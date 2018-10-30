using System.Collections.Generic;

namespace CardOrganizerWPF
{
    public static class GameInfo
    {
        public enum Game
        {
            HoneySelect,
            Koikatu,
            Playhome
        }

        public enum Path
        {
            Scene,
            Chara1,
            Chara2,
            Outfit1,
            Outfit2
        }

        public static Dictionary<string, Game> games = new Dictionary<string, Game>
        {
            {"HS", Game.HoneySelect},
            {"KK", Game.Koikatu},
            {"PH", Game.Playhome}
        };

        public static Dictionary<Path, string> HSPath = new Dictionary<Path, string>
        {
            {Path.Scene, @"studioneo\scene"},
            {Path.Chara1, @"chara\female"},
            {Path.Chara2, @"chara\male"},
            {Path.Outfit1, @"coordinate\female"},
            {Path.Outfit2, @"coordinate\male"}
        };

        public static Dictionary<Path, string> KKPath = new Dictionary<Path, string>
        {
            {Path.Scene, @"studio\scene"},
            {Path.Chara1, @"chara\female"},
            {Path.Chara2, @"chara\male"},
            {Path.Outfit1, @"coordinate"}
        };
    }
}
