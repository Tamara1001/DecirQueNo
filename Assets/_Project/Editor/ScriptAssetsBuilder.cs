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

            // Lines separated by a line break are shown as separate chat bubbles.
            public string Body;

            // Phrases of the board split in blocks with '|'. Each block is one button of the bank.
            public string Yes;
            public string Maybe;
            public string Evasive;
            public string No;
            public string YesReply;
            public string EvasiveReply;
            public string NoReply;

            // The board sends the in-between answer to the same reply as the evasive one, except in the final message.
            public string MaybeReply;
        }

        private static readonly Tree[] Trees =
        {
            new Tree
            {
                Name = "Msg_1_1", Sender = Valerie, Level = 1,
                Body = "Obvio me ubicas, así que iré directo al punto.\nVi tu video en mi FYP y tu perfil es lo que he estado buscando.\n¿Estás lista para crecer en serio? Con el hype que traigo fácil llegas al 1 millón de seguidores...\n¿Tienes lo necesario para seguirme el paso o te me vas a quedar en el camino? 💅✨",
                Yes = "¡Obvio|sí,|listísima!",
                Maybe = "Lo|voy a|pensar...",
                Evasive = "Suena bien,|pero prefiero|ir paso|a pasito.",
                YesReply = "¡Así me gusta, con ganas y actitud!\nTe paso las instrucciones por privado en dos minutos.\nArréglate hermosa y no me vayas a dejar en ridículo.",
                EvasiveReply = "Ay corazón, qué flojera me dan las personas inseguras.\nConmigo vas al cien o le paso esta oportunidad a otra persona.\nTe paso las pautas, no me hagas perder el tiempo.",
            },
            new Tree
            {
                Name = "Msg_1_2", Sender = Valerie, Level = 1,
                Body = "Oye linda 💋\nRevisé tu perfil y está aburridísimo.\nEn el live vamos a fingir una pelea y lloras en cámara o no te vuelvo a invitar. 💅",
                Yes = "¡Me|encanta!",
                Maybe = "No estoy|muy|segura",
                Evasive = "¿Y si|mejor|hacemos|algo divertido|sin tanto|drama?",
                YesReply = "Esa es la actitud, hermosa.\nTres minutos y te mando el enlace, arréglate bien 💄✨",
                EvasiveReply = "Ay no, qué pereza tu inseguridad.\nSi vas a llorar de verdad mejor busco a otra 🙄",
            },
            new Tree
            {
                Name = "Msg_1_3", Sender = Valerie, Level = 1,
                Body = "Estuvo brutal el morbo del live, rompimos récord.\nYa cumplí contigo, ahora le paso tu contacto a Christian Vega, mi manager.\nEscríbele de una vez o perderás el momento.",
                Yes = "Le|escribo|ahora.",
                Maybe = "Lo busco|en un|rato.",
                Evasive = "No le|voy a|escribir|a nadie.",
                YesReply = "De nada, hermosa, te lo ganaste.\nCobra caro pero hace milagros, no me hagas quedar mal ni lo dejes esperando 👠✨",
                EvasiveReply = "Escríbele ahorita o busca otro manager por tu cuenta 🥱",
            },
            new Tree
            {
                Name = "Msg_2_1", Sender = Christian, Level = 2,
                Body = "Buenas noches, Samantha.\nValerie me pasó tu contacto. Veo potencial comercial en tu perfil.\nPara representarte exijo disponibilidad total 24/7.\nSi tienes clases o compromisos, los cancelas ya.",
                Yes = "Sí,|cancelo|todo.",
                Maybe = "Déjame|revisar|mis horarios.",
                Evasive = "No voy|a cancelar|toda|mi vida.",
                YesReply = "Sabía que eras inteligente. Tienes un brillo único que Valerie jamás tuvo.\nTe envié el cronograma a tu correo. No me falles.",
                EvasiveReply = "Samantha, tienes un talento demasiado grande como para desperdiciarlo en cosas mediocres.\nNo puedo ayudarte si tú misma no te la crees. Cancela tus compromisos y enfócate en tu contenido.",
            },
            new Tree
            {
                Name = "Msg_2_2", Sender = Christian, Level = 2,
                Body = "Samantha, estuve revisando tus publicaciones.\nTus amigos te dejan comentarios que arruinan tu feed y tus posteos.\nYa estás a otro nivel, no necesitas chismes ni ese tipo de interacciones,\nlo mejor es bloquearlos ya, antes de que nos generen un problema.",
                Yes = "Sí,|los|bloqueo.",
                Maybe = "Solo voy|a|silenciarlos.",
                Evasive = "Esto|realmente|me incomoda,|no quiero|bloquearlos.",
                YesReply = "Esa es la madurez que te hace diferente al resto.\nSé que cuesta soltar el pasado, pero tu talento merece un público de verdad. Confía en mí.",
                EvasiveReply = "Samantha, esa sensibilidad tuya es hermosa, pero en los negocios te hace vulnerable.\nNo lo veas como algo cruel, te estoy protegiendo de la envidia, verás que tengo razón.",
            },
            new Tree
            {
                Name = "Msg_2_3", Sender = Christian, Level = 2,
                Body = "Te conseguí al patrocinador más exclusivo del medio, linda.\nPagó una fortuna por una transmisión privada contigo esta madrugada.\nLe pasé tu dirección para los regalos de la marca.\nConéctate y sé encantadora.",
                Yes = "Me|conecto|ahora.",
                Maybe = "¿Por qué|diste|mi dirección?",
                Evasive = "No voy|a hacer|esa|transmisión|privada.",
                YesReply = "Sabía que verías el panorama completo. Eres una profesional nata.\nEste cliente nos va a hacer millonarios. Te hablará en breve, sé impecable.",
                EvasiveReply = "¿Desconfías de mí después de todo lo que he invertido para cuidarte?\nEres brillante, no dejes que el miedo te sabotee justo cuando estamos por ganar. Entra al enlace ahora.",
            },
            new Tree
            {
                Name = "Msg_3_1", Sender = Vip, Level = 3,
                Body = "Buenas noches, Samantha. Disculpa la hora.\nLlevo semanas siguiendo tu contenido. Tienes una sensibilidad que hace mucha falta en redes.\nAcordé con Christian patrocinar tu próximo proyecto.\nMe encantaría una videollamada breve para conocerte mejor.",
                Yes = "Sí,|me|conecto.",
                Maybe = "¿Podemos|dejarlo|para|mañana?",
                Evasive = "No voy|a hacer|videollamadas|a esta|hora.",
                YesReply = "Perfecto, Samantha.\nTe envío el enlace privado.\nAsegúrate de estar completamente sola, quiero toda tu atención.",
                EvasiveReply = "No sientas pena, Samantha, el talento no tiene horario.\nQué lástima ver que te falta ambición.\nChristian me aseguró que tu disponibilidad era total.",
            },
            new Tree
            {
                Name = "Msg_3_2", Sender = Vip, Level = 3,
                Body = "Eres aún más fascinante sin todo el maquillaje que te impone Christian.\nTe ves nerviosa mirando hacia la cortina... la dejaste entreabierta.\nEstoy afuera de tu edificio. Baja a abrirme, los proyectos grandes se firman de frente.",
                Yes = "Sí,|ya|bajo.",
                Maybe = "No sé|si|debería.",
                Evasive = "No voy|a bajar,|vete|de aquí|ahora.",
                YesReply = "Esa es mi estrella.\nNo tardes, hace frío aquí afuera y tenemos mucho de qué hablar a solas.",
                EvasiveReply = "La confianza se construye cara a cara.\nMe obligas a hacer unas cuantas llamadas para que Christian te quite todo lo que tienes.\nUn vecino me abrió la puerta principal del edificio. Voy subiendo.",
            },
            new Tree
            {
                Name = "Msg_3_3", Sender = Vip, Level = 3, IsFinal = true,
                Body = "Buenos días, Samantha.\nLo de anoche fue solo el comienzo.\nGraba un video ahora. Tu audiencia ama verte destrozada, el morbo vende más que cualquier otra cosa.",
                Yes = "Sí,|lo|grabo.",
                Maybe = "Déjame|respirar|un momento.",
                Evasive = "",
                No = "No, no|voy a|abrir|la cámara,|no voy|a|exponerme.",
                YesReply = "Final de la ruta\n\nYa que más da, si ya perdí todo, incluso a mí misma...\n\nMás números, más vistas y eso significa que no estoy sola...",
                MaybeReply = "Final de la ruta\n\nYa que más da, si ya perdí todo, incluso a mí misma...\n\nMás números, más vistas y eso significa que no estoy sola...",
                NoReply = "Final de la ruta\n\nY si eso significa perder la cuenta, el dinero, los contratos y quedarme sola sin nada después de haber destruido todo por complacerlos, que así sea. Hasta aquí llegué...\n\n¡NO!",
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
            message.maybeWords = Split(tree.Maybe);
            message.evasiveWords = Split(tree.Evasive);
            message.noWords = Split(tree.No);
            message.yesReply = tree.YesReply ?? "";
            message.evasiveReply = tree.EvasiveReply ?? "";
            message.maybeReply = tree.MaybeReply ?? message.evasiveReply;
            message.noReply = tree.NoReply ?? "";

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
                new NoLevelConfig(70, 1f, BankArrangement.Ordered),
                new NoLevelConfig(40, 0.9f, BankArrangement.Scrambled),
                new NoLevelConfig(0, 0.8f, BankArrangement.Mixed),
            };
            config.responseSeconds = 46f;
            config.musicVolume = 0.5f;
            config.sfxVolume = 0.8f;
            config.musicCrossfadeSeconds = 2.5f;
            config.goodMaxYes = 3;
            config.goodMinNegativeShare = 0.5f;
            config.levels = new List<AntagonistLevelConfig>
            {
                new AntagonistLevelConfig(120000, 60000, -3000, -15, -7, 0),
                new AntagonistLevelConfig(140000, 70000, -5000, -20, -10, 5),
                new AntagonistLevelConfig(155000, 77500, -10000, -25, -12, 5),
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
                day.dayLength = 150f;
                EditorUtility.SetDirty(day);
            }

            EditorUtility.SetDirty(config);
        }

        // Each block of a phrase (separated by '|') is one button of the bank.
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
