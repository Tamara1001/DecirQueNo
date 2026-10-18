using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DecirQueNo.EditorTools
{
    // Generates the MessageData, MessagePool, DayData and GameConfig values from GUION.md.
    // Edit the table here when the writers change the board, then run the menu item again.
    public static class ScriptAssetsBuilder
    {
        private const string MessagesFolder = "Assets/_Project/Data/Messages";
        private const string PoolPath = "Assets/_Project/Data/MessagePool.asset";
        private const string ConfigPath = "Assets/_Project/Resources/GameConfig.asset";

        private const string Valerie = "Valerie";
        private const string Christian = "Christian";
        private const string Vip = "Contacto VIP";

        private class Tree
        {
            public string Name;
            public string Sender;
            public int Level;
            public bool IsFinal;
            public string Body;
            public string Yes;
            public string Evasive;
            public string No;
            public string YesReply;
            public string EvasiveReply;
            public string NoReply;

            // Fragments as written in the Miro board. Null when they are the same as the phrases.
            public string BoardBank;
        }

        private static readonly Tree[] Trees =
        {
            new Tree
            {
                Name = "Msg_1_1", Sender = Valerie, Level = 1,
                Body = "Obvio me ubicas, así que ire directo al punto. Vi tu video en mi FYP y tu perfil es lo que he estado buscando ¿Estás lista para crecer en serio? Con el hype que traigo fácil llegas al 1 millon de seguidores... ¿Tienes lo necesario para seguirme el paso o te me vas a quedar en el camino? 💅✨",
                Yes = "¡Obvio sí! 💖|¡no inventes,|estoy listísima,|te sigo|el paso! ✨",
                Evasive = "Suena increíble,|pero prefiero|ir paso|a pasito,|¿te parece? 🥺✨",
                YesReply = "¿Así me gusta, con ganas y actitud! Te paso las instrucciones por privado en dos minutos. Arréglate hermosa y no me vayas a dejar en ridículo.",
                EvasiveReply = "Ay corazón, qué flojera me da las personas inseguras. Conmigo vas al cien o le paso esta opotunidad a otra persona. Te paso las pautas, no me hagas perder el tiempo.",
            },
            new Tree
            {
                Name = "Msg_1_2", Sender = Valerie, Level = 1,
                Body = "Oye linda 💋\nRevisé tu perfil y está aburridísimo\nEn el live vamos a fingir una pelea y lloras en cámara o no te vuelvo a invitar. 💅",
                Yes = "¡Me encanta! 🤩|¡yo actúo|todo de|una vez,|cuenta conmigo! 🙌",
                Evasive = "¿Y si|mejor hacemos|algo divertido|sin tanto|drama? 🙈",
                YesReply = "Esa es la actitud hermosa.\nTres minutos y te mando el enlace, arréglate bien 💄✨",
                EvasiveReply = "Ay no, qué pereza tu inseguridad\nSi vas a llorar de verdad mejor busco a otra 🙄",
            },
            new Tree
            {
                Name = "Msg_1_3", Sender = Valerie, Level = 1,
                Body = "Estuvo brutal el morbo del live, rompimos record.\nYa cumplí contigo, ahora le paso tu contacto a Christian Vega, mi manager\nEscríbele de una vez o perderás el momento",
                Yes = "¡No inventes! 😱|¡muchísimas gracias,|ya mismo|le hablo,|¡mil gracias!",
                Evasive = "Quedé agotadísima,|¿puedo hablarle|mañana con|un poco|más calma?",
                YesReply = "De nada hermosa, te lo ganaste\nCobra caro pero hace milagros, no me hagas quedar mal ni lo dejes esperando 👠✨",
                EvasiveReply = "¿Descansar? Ay linda, qué ingenua eres\nLa gente importante no espera tu siesta.\nEscríbele ahorita o busca otro manager por tu cuenta 🥱",
            },
            new Tree
            {
                Name = "Msg_2_1", Sender = Christian, Level = 2,
                Body = "Buenas noches, Samantha\nValerie me pasó tu contacto. Veo pontencial comercia en tu perfil.\nPara representarte exijo disponibilidad total 24/7. Si tienes clases o compromisos, los cancelas ya.",
                Yes = "Tengo tiempo|total,|estoy a|su entera|disposición,|señor Christian. 🙏",
                Evasive = "Tengo compromisos|previos, pero|intentaré acomodar|mis horarios, ⏳|señor Vega. 🙇‍♀️",
                YesReply = "Sabía que eras inteligente. Tienes un brillo único que Valerie jamás tuvo.\nTe envié el cronograma a tu correo. No me falles.",
                EvasiveReply = "Samantha, tienes un talento demasiado grande como para desperdiciarlo en cosas mediocres.\nYo veo en ti mucho potencial, no puedo ayudarte si tú misma no te la crees. Cancela tus compromisos y enfócate en tu futura carrera de influencer famosa.",
                BoardBank = "mis horarios, ⏳|estoy a|previos, pero|señor Christian. 🙏|Tengo tiempo|intentaré acomodar|su entera|Tengo compromisos|señor Vega. 🙇‍♀️|total, disposición,",
            },
            new Tree
            {
                Name = "Msg_2_2", Sender = Christian, Level = 2,
                Body = "Samantha, estuve revisando tus publicaciones.\nTus amigos te dejan comentarios que arruinan tu feed y posteos.\nYa estas a otro nivel, no necesitas chimes y ese tipo de interacciones, lo mejor es bloquearlos ya, antes de que nos generen un problema.",
                Yes = "Está bien,|los silencio|por ahora,|si es|necesario. 🙇‍♀️",
                Evasive = "Esto realmente|me incomoda,|pero si|usted insiste...|lo haré. 🥺",
                YesReply = "Esa es la madurez que te hace diferente al resto.\nSé que cuesta soltar el pasado, pero tu talento merece un público de verdad. Confía en mí.",
                EvasiveReply = "Samantha, esa sensibilidad tuya es hermosa, pero en los negocios te hace vulnerable.\nNo lo veas como algo cruel, te estoy protegiendo de la envidia, verás que tengo razón.",
            },
            new Tree
            {
                Name = "Msg_2_3", Sender = Christian, Level = 2,
                Body = "Te conseguí al patrocinador más exclusivo del medio, linda.\nPagó una fortuna por una transmisión privada contigo esta madrugada.\nLe pasé tu dirección para los regalos de la marca.\nConéctate y sé encantadora.",
                Yes = "Cumpliré con|la llamada,|supongo que|no queda|otra. 😰",
                Evasive = "Me asusta|demasiado esto...|pero me|conectaré hoy. 🤝|está bien,",
                YesReply = "Sabía que verías el panorama completo. Eres una profesional nata\nEste cliente tiene el poder de ponerte en las grandes ligas. Te hablará en breve, sé impecable.",
                EvasiveReply = "¿Desconfías de mí después de todo lo que he invertido para cuidarte?\nEres brillante, no dejes que el miedo te sabotee justo cuando estamos por ganar. Entra al enlace ahora.",
            },
            new Tree
            {
                Name = "Msg_3_1", Sender = Vip, Level = 3,
                Body = "Buenas noches, Samantha. Disculpa la hora.\nLlevo semanas siguiendo tu contenido. Tienes una sensibilidad que hace mucha falta en redes.\nAcordé con Christian patrocinar tu próximo proyecto. Me encantaría una videollamada breve para conocerte mejor.",
                Yes = "ya entro|pero esto|me da|pena tan|tarde igual",
                Evasive = "agradezco el|apoyo pero|me da|desconfianza la|hora señor",
                YesReply = "No sientas pena, Samantha, el talento no tiene horario.\nEs solo una charla entre profesionales. Abre el enlace cuando gustes.",
                EvasiveReply = "Entiendo tus precauciones, habla muy bien de ti.\nChristian te garantizó mi seriedad y la cifra pactada. Entra un momento, por favor.",
            },
            new Tree
            {
                Name = "Msg_3_2", Sender = Vip, Level = 3,
                Body = "Eres aún más fascinante sin todo el maquillaje que te impone Christian.\nTe ves nerviosa mirando hacia la cortina... la dejaste entreabierta.\nEstoy afuera de tu edificio. Baja a abrirme, los proyectos grandes se firman de frente.",
                Yes = "ya bajo|pero que|no demore|mucho ya|es tarde",
                Evasive = "no deberías|estar aquí|es demasiado|es necesario|de frente",
                YesReply = "Solo tomará unos minutos, Samantha.\nAbre la reja principal. Ya crucé la acera.",
                EvasiveReply = "Por supuesto que es necesario; la confianza se construye cara a cara.\nUn vecino me abrió la puerta principal del edificio. Voy subiendo.",
            },
            new Tree
            {
                Name = "Msg_3_3", Sender = Vip, Level = 3, IsFinal = true,
                Body = "Buenos días, Samantha.\nLo de anoche fue solo el comienzo.\nGraba un video ahora. Tu audiencia ama verte destrozada, el morbo vende más que cualquier otra cosa.",
                Yes = "Está bien,|he hecho|de todo|para llegar|hasta aquí,|ya no|me queda|más que|ceder ahora.",
                Evasive = "",
                No = "No, no|voy a|abrir|la cámara,|no voy|a exponerme|ante nadie|y no|voy a|contestar|un solo|mensaje más.",
                YesReply = "Final de la ruta\n\nYa que más da, si ya perdí todo incluso a mi misma..\n\nMás números, más vistas y eso significa que no estoy sola...",
                NoReply = "Final de la ruta\n\nY si eso significa perder la cuenta, el dinero, los contratos y quedarme sola sin nada después de haber destruido todo por complacerlos, que así sea. Hasta aquí llegué..\n\nNO",
                BoardBank = "la cámara,|no voy|un solo|Está bien,|a exponerme|he hecho|mensaje más.|ante nadie|de todo|para llegar|No, no|y no|hasta aquí,|ya no|voy a|me queda|más que|ceder ahora.|abrir|contestar",
            },
        };

        [MenuItem("DecirQueNo/Build/Generate script assets (GUION.md)")]
        public static void Generate()
        {
            ClearMessagesFolder();

            var messages = new List<MessageData>();
            foreach (Tree tree in Trees)
            {
                messages.Add(CreateMessage(tree));
            }

            var pool = AssetDatabase.LoadAssetAtPath<MessagePool>(PoolPath);
            pool.messages = messages;
            EditorUtility.SetDirty(pool);

            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            ApplyConfig(config, pool);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ScriptAssetsBuilder] {messages.Count} messages, {config.days.Count} days.");
        }

        private static void ClearMessagesFolder()
        {
            if (!AssetDatabase.IsValidFolder(MessagesFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Messages");
                return;
            }

            foreach (string guid in AssetDatabase.FindAssets("t:MessageData", new[] { MessagesFolder }))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        private static MessageData CreateMessage(Tree tree)
        {
            var message = ScriptableObject.CreateInstance<MessageData>();
            message.sender = tree.Sender;
            message.level = tree.Level;
            message.isFinal = tree.IsFinal;
            message.body = tree.Body;
            message.yesWords = Split(tree.Yes);
            message.evasiveWords = Split(tree.Evasive);
            message.noWords = Split(tree.No);
            message.yesReply = tree.YesReply ?? "";
            message.evasiveReply = tree.EvasiveReply ?? "";
            message.noReply = tree.NoReply ?? "";

            if (tree.BoardBank != null)
            {
                message.sourceBank = Split(tree.BoardBank);
            }
            else
            {
                message.sourceBank = new List<string>();
                message.sourceBank.AddRange(message.yesWords);
                message.sourceBank.AddRange(message.evasiveWords);
                message.sourceBank.AddRange(message.noWords);
            }

            AssetDatabase.CreateAsset(message, Path.Combine(MessagesFolder, tree.Name + ".asset"));
            return message;
        }

        private static void ApplyConfig(GameConfig config, MessagePool pool)
        {
            config.startFollowers = 320;
            config.goalFollowers = 1000000;
            config.startHealth = 70;
            config.maxHealth = 100;
            config.noLevels = new List<NoLevelConfig>
            {
                new NoLevelConfig(70, 1f),
                new NoLevelConfig(40, 0.8f),
                new NoLevelConfig(0, 0.6f),
            };
            config.timerMinSeconds = 20f;
            config.timerMaxSeconds = 60f;
            config.levels = new List<AntagonistLevelConfig>
            {
                new AntagonistLevelConfig(30000, -3000, -15, 0),
                new AntagonistLevelConfig(100000, -10000, -20, 5),
                new AntagonistLevelConfig(230000, -23000, -25, 5),
            };
            config.noFollowers = -100000;
            config.noHealth = 30;
            config.notificationLifetime = 30f;
            config.expireFollowersMultiplier = 1.5f;
            config.messagesPerDay = new List<int> { 3, 3, 3 };
            config.showEmojis = false;
            config.pool = pool;

            foreach (DayData day in config.days)
            {
                day.dayLength = 240f;
                EditorUtility.SetDirty(day);
            }

            EditorUtility.SetDirty(config);
        }

        private static List<string> Split(string fragments)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(fragments))
            {
                return list;
            }

            foreach (string part in fragments.Split('|'))
            {
                list.Add(part);
            }

            return list;
        }
    }
}
