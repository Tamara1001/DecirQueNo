import os, shutil, subprocess

FPS = 24
ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Trailer")
os.chdir(ROOT)
RES = "D:/Archivos/Unity/DecirQueNo/Assets/_Project/Resources"
FONT = "Fredoka.ttf"
shutil.copy(f"{RES}/Fonts/Fredoka.ttf", FONT)
os.makedirs("seg", exist_ok=True)
os.makedirs("txt", exist_ok=True)

M = {l.split()[0]: int(l.split()[1]) for l in open("markers.txt")}


def run(args):
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error"] + args, check=True)


def txt(name, text):
    p = f"txt/{name}.txt"
    open(p, "w", encoding="utf-8").write(text)
    return p


def caption_png(name, text, size=56):
    """Caption in the game's look: lilac rounded card, navy border, Fredoka, drawn with PIL."""
    from PIL import Image, ImageDraw, ImageFont
    font = ImageFont.truetype(FONT, size)
    tw = int(font.getlength(text))
    w, h = tw + 120, size + 70
    sc = 2
    img = Image.new("RGBA", (w * sc, h * sc), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((0, 0, w * sc - 1, h * sc - 1), radius=34 * sc, fill=(5, 32, 61, 255))
    d.rounded_rectangle((7 * sc, 7 * sc, w * sc - 8 * sc, h * sc - 8 * sc), radius=28 * sc, fill=(250, 204, 245, 255))
    f2 = ImageFont.truetype(FONT, size * sc)
    d.text((w * sc / 2, h * sc / 2 + 2 * sc), text, font=f2, fill=(5, 32, 61, 255), anchor="mm")
    img = img.resize((w, h), Image.LANCZOS)
    p = f"txt/{name}.png"
    img.save(p)
    return p


def caption(name, text, start, dur, size=54, y="h-150"):
    """drawtext fragment with a fade in/out and a soft box."""
    p = txt(name, text)
    a = f"if(lt(t,{start}),0,if(lt(t,{start}+0.4),(t-{start})/0.4,if(lt(t,{start}+{dur}-0.4),1,max(0,({start}+{dur}-t)/0.4))))"
    return (f"drawtext=fontfile={FONT}:textfile={p}:fontcolor=white:fontsize={size}:x=(w-text_w)/2:y={y}"
            f":box=1:boxcolor=0x1d1030@0.55:boxborderw=22:alpha='{a}'")


def clip(name, first, last, speed, cap=None, fade=0.3):
    """Frames first..last of the recording sped up by `speed` -> seg/name.mp4"""
    n = last - first
    out_dur = n / FPS / speed
    vf = (f"setpts=PTS/{speed},fps={FPS},scale=1920:1080:flags=lanczos,format=yuv420p"
          f",fade=t=in:st=0:d={fade},fade=t=out:st={out_dur - fade:.3f}:d={fade}")
    src = ["-framerate", str(FPS), "-start_number", str(first), "-i", "frames/f%06d.jpg"]
    if cap:
        png, st, cd = cap
        chain = (f"[0:v]{vf}[b];[1:v]format=rgba,fade=t=in:st={st}:d=0.4:alpha=1,fade=t=out:st={st + cd - 0.4}:d=0.4:alpha=1[c];"
                 f"[b][c]overlay=x=(W-w)/2:y=H-h-70:format=auto,format=yuv420p[v]")
        run(src + ["-loop", "1", "-framerate", str(FPS), "-i", png, "-filter_complex", chain, "-map", "[v]",
                   "-frames:v", str(int(out_dur * FPS)), "-c:v", "libx264", "-crf", "17", "-preset", "medium", f"seg/{name}.mp4"])
    else:
        run(src + ["-frames:v", str(int(out_dur * FPS)), "-vf", vf, "-c:v", "libx264", "-crf", "17", "-preset", "medium",
                   f"seg/{name}.mp4"])
    return out_dur


def card(name, dur, bg, lines, avatar=None, color="white"):
    """Solid colour card with optional avatar, text lines [(text,size,y)]."""
    vf = []
    inputs = ["-f", "lavfi", "-i", f"color=c={bg}:s=1920x1080:r={FPS}:d={dur}"]
    if avatar:
        inputs += ["-loop", "1", "-framerate", str(FPS), "-t", str(dur), "-i", avatar]
        chain = "[1:v]scale=-1:440[av];[0:v][av]overlay=x=(W-w)/2:y=150[b]"
        last = "b"
    else:
        chain = "[0:v]null[b]"
        last = "b"
    for i, (t, size, y) in enumerate(lines):
        p = txt(f"{name}_{i}", t)
        nxt = f"t{i}"
        chain += (f";[{last}]drawtext=fontfile={FONT}:textfile={p}:fontcolor={color}:fontsize={size}"
                  f":x=(w-text_w)/2:y={y}[{nxt}]")
        last = nxt
    chain += f";[{last}]fade=t=in:st=0:d=0.3,fade=t=out:st={dur - 0.3}:d=0.3,format=yuv420p[v]"
    run(inputs + ["-filter_complex", chain, "-map", "[v]", "-c:v", "libx264", "-crf", "17", f"seg/{name}.mp4"])
    return dur


order = []
dur = {}


def add(name, d):
    order.append(name)
    dur[name] = d


# 1. title
add("title", card("title", 3.6, "0x06203F", [("Still Typing...", 150, "(h-text_h)/2-90"),
                                             ("Un juego sobre aprender a decir que no", 54, "(h-text_h)/2+80"),
                                             ("Hecho para la Women Game Jam 2026", 40, "(h-text_h)/2+170")], color="0xFACCF5"))

# 2. menu + tutorial
add("menu", clip("menu", M["menu"] + 10, M["menu"] + 100, 1.0))
add("instr", clip("instr", M["instructions"] + 20, M["instructions"] + 330, 3.0))

# 3. day 1, slow human typing
add("day1", clip("day1", M["day1"] + 250, M["day1"] + 250 + 420, 1.5,
                 (caption_png("c_day1", "Todos quieren un sí"), 0.8, 3.2)))

# 4. the three voices
add("valerie", card("valerie", 2.5, "0xD786CF", [("Valerie", 84, 640), ("\"Escríbele de una vez o perderás el momento.\"", 46, 780)],
                    "valerie_new.png"))
add("christian", card("christian", 2.5, "0x0092B4", [("Christian", 84, 640), ("\"Si tienes compromisos, los cancelas ya.\"", 46, 780)],
                      f"{RES}/UI/Avatar_Christian.png"))
add("vip", card("vip", 2.5, "0x001D40", [("Contacto VIP", 84, 640), ("\"Estoy afuera de tu edificio. Baja a abrirme.\"", 46, 780)],
                f"{RES}/UI/Avatar_Vip.png"))

# 5. low mental health
add("low", clip("low", M["day2_low"] + 140, M["day2_low"] + 140 + 340, 1.4,
                (caption_png("c_low", "Cada sí te cuesta salud mental"), 0.8, 4.5)))

# 6. night / mirror
add("sleep", clip("sleep", M["sleep2_low"] + 80, M["sleep2_low"] + 80 + 280, 2.0))

# 7. endings: bad, neutral, good
add("end_sad", clip("end_sad", M["ending_sad"] - 90, M["ending_sad"] + 140, 1.5))
add("end_neutral", clip("end_neutral", M["ending_neutral"] - 90, M["ending_neutral"] + 140, 1.5))
add("end_happy", clip("end_happy", M["ending_happy"] - 90, M["ending_happy"] + 140, 1.5))

# 8. end card
add("outro", card("outro", 5.0, "0x06203F", [("Still Typing...", 130, 330),
                                             ("¿Te animás a decir que no?", 56, 520),
                                             ("Jugalo en itch.io", 60, 650),
                                             ("Hecho para la Women Game Jam 2026", 38, 860)], color="0xFACCF5"))

# concat video
with open("seg/list.txt", "w") as f:
    for n in order:
        f.write(f"file '{n}.mp4'\n")
run(["-f", "concat", "-safe", "0", "-i", "seg/list.txt", "-c", "copy", "seg/video.mp4"])

# ---- audio ----
A = f"{RES}/Audio"
t_menu = sum(dur[n] for n in ["title", "menu", "instr", "day1", "valerie", "christian", "vip"])
t_low = dur["low"] + dur["sleep"]
t_end = dur["end_sad"] + dur["end_neutral"] + dur["end_happy"] + dur["outro"]
parts = [("BGM_Menu", t_menu), ("BGM_Poca_Salud", t_low), ("BGM_Ganar", t_end)]
inputs, chain, labels = [], "", ""
for i, (n, d) in enumerate(parts):
    inputs += ["-i", f"{A}/{n}.ogg"]
    chain += f"[{i}:a]aloop=loop=-1:size=2147483647,atrim=0:{d:.3f},asetpts=PTS-STARTPTS,afade=t=in:st=0:d=0.8,afade=t=out:st={d - 0.8:.3f}:d=0.8,volume=0.9[a{i}];"
    labels += f"[a{i}]"
# notification pings on the three character cards
t0 = sum(dur[n] for n in ["title", "menu", "instr", "day1"])
inputs += ["-i", f"{A}/SFX_Notificacion.ogg"] * 3
for k in range(3):
    idx = len(parts) + k
    ms = int((t0 + k * 2.5 + 0.1) * 1000)
    chain += f"[{idx}:a]adelay={ms}|{ms},volume=1.2[p{k}];"
chain += f"{labels}concat=n={len(parts)}:v=0:a=1[bgm];[bgm][p0][p1][p2]amix=inputs=4:duration=first:normalize=0[aout]"
run(inputs + ["-filter_complex", chain, "-map", "[aout]", "-c:a", "aac", "-b:a", "192k", "seg/audio.m4a"])

run(["-i", "seg/video.mp4", "-i", "seg/audio.m4a", "-c:v", "copy", "-c:a", "copy", "-shortest", "trailer.mp4"])
print("total", sum(dur.values()))
