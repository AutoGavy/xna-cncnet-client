using Localization;
using Microsoft.Xna.Framework.Media;

namespace DTAClient.DXGUI.Generic
{
    public class InfoShared
    {
        public static readonly string[] DifficultyNames = new string[]
        {
            "Easy".L10N("UI:Main:Easy"),
            "Normal".L10N("UI:Main:Normal"),
            "Hard".L10N("UI:Main:Hard"),
            "Abyss".L10N("UI:Main:Abyss")
        };

        public static string[] DifficultyIniPaths = new string[]
        {
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/Difficulty Normal.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Hell.ini"
        };

        public static readonly string[] filesHashArrayCamp =
        {
            "A851224274A86CB39217DA2D38E67E7DE4C8C611", // Difficulty Easy.ini
            "96F69F6CBAE1146B5105F70E46F12E6DF506A9EB", // Difficulty Hard.ini
            "7313144376C4385B8F1ED09E4BC850752AFB4735", // Difficulty Hell.ini
            "00637DA534D5ABF27C56D72E9D0DA818AFE2C941", // Difficulty Normal.ini
            "DB0CE24FD8FE75E5E7C03649D04D17E14483A24D", // GlobalCode.ini

            "8F04CA0591D7EFA927CAB025BB78DE247526EBEF", // Train 00
            "75AC924E69177C2C3A340B2234BDE1AC1A60D4AF", // Train 01
            "3718245B8DA961AE152A27381F8E930F4EDA22E2", // Train 02
            "C3D18E72CD2F2DFDD3817F563E3A25E0CF025D19", // Train 03

            "14E2C2CCD0B59CE50F493611133D6A5A2A2DD502", // GDI 01
            "C7E0A19C3BDDCDAB43255668FE62A4FCE73256B0", // GDI 02
            "EDC997F50D73DEC1DA1F56D112F0675CBF948661", // GDI 03
            "0BE03DBDA4C29BA276E146E60EACEB3646ADE5A2", // GDI 04
            "506464800EEC49F2936D477E0624626258A6EC7B", // GDI 05
            "7C16B5B88C726DD9F640DC7DB35B6451C92ADB5B", // GDI 06
            "B92A11D49285852FC0D622CA5F79C509FDE8B69D", // GDI 07
            "E13C311EA0B0B5CB1502F72A3F377E98654EC2B6", // GDI 08

            "7B071F82A931054961370F177738CEE45883AF4C", // End 01
            "337617CC5B7AFB6AD92B548B273B655A91FF54C0", // End 02

            "71751972AC0C393FD9CF31A86CD96111CB010D18", // Nod 01
            "A68ADE548D44FA812BD49D47E27A9BD10EE3C228", // Nod 02
            "7B7B17B7370C02473306E1531370027120B99A3C", // Nod 03
            "1AA77A203A327A36A0EBBEDDAE03E2A8458F01BE", // Nod 04
            "8F8051E06C48A381D4C4AA8CBCA5254AE2450335", // Nod 05
            "E60363ACFAEC22B9093754EC773E40B386281DCE", // Nod 06
            "3DCCBAB4FF70498A2550862FD0713F3164340280", // Nod 07
            "CEA84166EBB0E29738E37A524E9D8249A97C73C1", // Nod 08

            "1A08B84E5F903E51C32B0AF3996790F0478C4840", // Scrin 01
            "D33A4456B17871D4014FFFADCB4598C6951C8047", // Scrin 02
            "2DA6B42A4E6B5AC6E1F054EAB8D42F029F43008B", // Scrin 03
            "92D8CDC82DFBF3C078E2431B3380643F9E165D17"  // Scrin 04
        };

        public static readonly string[] filesToCheckCamp =
        {
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Hell.ini",
            "INI/Map Code/Difficulty Normal.ini",
            "INI/Map Code/GlobalCode.ini",

            "MapsTC/Missions/tra00.map",
            "MapsTC/Missions/tra01.map",
            "MapsTC/Missions/tra02.map",
            "MapsTC/Missions/tra03.map",

            "MapsTC/Missions/gdi01.map",
            "MapsTC/Missions/gdi02.map",
            "MapsTC/Missions/gdi03.map",
            "MapsTC/Missions/gdi04.map",
            "MapsTC/Missions/gdi05.map",
            "MapsTC/Missions/gdi06.map",
            "MapsTC/Missions/gdi07.map",
            "MapsTC/Missions/gdi08.map",

            "MapsTC/Missions/end01.map",
            "MapsTC/Missions/end02.map",

            "MapsTC/Missions/nod01.map",
            "MapsTC/Missions/nod02.map",
            "MapsTC/Missions/nod03.map",
            "MapsTC/Missions/nod04.map",
            "MapsTC/Missions/nod05.map",
            "MapsTC/Missions/nod06.map",
            "MapsTC/Missions/nod07.map",
            "MapsTC/Missions/nod08.map",

            "MapsTC/Missions/scr01.map",
            "MapsTC/Missions/scr02.map",
            "MapsTC/Missions/scr03.map",
            "MapsTC/Missions/scr04.map"
        };

        public static readonly string[] campaignList =
        {
            "tra00.map",
            "tra01.map",
            "tra02.map",
            "tra03.map",

            "prl01.map",
            "prl02.map",

            "gdo01.map",
            "gdo02.map",
            "gdo03.map",
            "gdo04.map",
            "gdo05.map",
            "gdo06.map",
            "gdo07.map",
            "gdo08.map",
            "gdo09.map",
            "gdo10.map",
            "gdo11.map",
            "gdo12.map",

            "gdi01.map",
            "gdi02.map",
            "gdi03.map",
            "gdi04.map",
            "gdi05.map",
            "gdi06.map",
            "gdi07.map",
            "gdi08.map",

            "end01.map",
            "end02.map",
            "end03.map",
            "end04.map",
            "end05.map",
            "end06.map",
            "end07.map",
            "end08.map",

            "nod01.map",
            "nod02.map",
            "nod03.map",
            "nod04.map",
            "nod05.map",
            "nod06.map",
            "nod07.map",
            "nod08.map",

            "scr01.map",
            "scr02.map",
            "scr03.map",
            "scr04.map"
        };

        public static readonly string[] LightNameArray =
        {
            "Red",
            "Blue",
            "Green",
            "Level",
            "Ground",
            "Ambient"
        };

        public static readonly string[] filesHashArray =
        {
            "e9189b4a4a48ffea8c1713081319d11bfc3cc54d", // Maps Settings
            "d97a4b3d959c6ce423889164d72784f396fd8e37", // GameOptions

            "6C0F1F00BDC1EE2CDAC67119E21CA816CF30271E", // Auto Handle
            "715869652AFC3D5E7C0102CB573CE586FD1174ED", // Auto Repair
            "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", // Ion Storm
            "3FDFE5A78683E1440F174F55E1EB826D39563571", // Destroyable Bridges
            "1926B0FA2971CCE3981A7BE7BEDDED8E96CD2C1D", // No Epic
            "A6A0245C282FD80454CD95AF1B4CC3A9E15D39D9", // Low Detail
            "972DC58682F890201409476BE6C950D64806141D", // No Muzzle
            "B2551F45259872B01093A317E304FF2A82BAC30D", // No Rain
            "440C94AC323DCE05232AB79352095D4F6AF07B10", // No Rush
            "DD7B28C0E59A907C25E94CFD2DED42B3E7233A9B", // No Silos
            "B2B71102BB35EC095CC9C1D038B7A1958B17C0D8", // No TEU Lift
            "3B305AE3A6E803A82DF6506B84D10787A9D97BB5", // No Tiberium Lifeforms
            "3033513D21B7BBC4D6809D2AB263AF27D6C33E9D", // No Turret
            "18C0E1F5CFF19D0F3BE2DA2232B3FF26AED5A1B6", // OreDouble
            "64FCA65E80822C690E24748FEA497EF82639D619", // OreQuintuple
            "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", // OreStandard
            "D5493E148240DF4BF5E19B8F5CE6239AD289436F", // OreTriple
            "0CFBCB6D5428DA579183F121E103AAB2B4F00B4D", // Reveal Map
            "7DE809012A6CF96503B78FD9C83EA4E0E9E62470", // Second Tech
            "C600B33BCCE62DD46D74CC40267D602D867550A6", // Unrebuildable Tech

            "DE03851F67AB87F7965784D343B4C6F57B9041BF", // Challenge Easy.ini
            "0C8F4A2100E234E44AAF6A08A072BEAA0C71B84A", // Challenge Hard.ini
            "B7A54AEEC72BF6CCC4E89F46841A3FF9621D69D3", // Challenge Medium.ini
            "F091810065CF34C8D7F503D4D844F59F46DAC808", // Co-Op Easy.ini
            "95025F487A6FF22F596DD92B8FEB6F5BE574863E", // Co-Op Hard.ini
            "AFD6C3EB0EA8A572EA5EB42F6B49229A778E6149", // Co-Op Medium.ini
            "E42E4B2478B7CFF648DCCD5BEAC8C8B1C7122E25", // Crazy Crates.ini
            "ec8df6503caba20c53fc4f42da431b4807774fae", // Difficulty Tier.ini
            "D42ECB8AE645547E361AF96DB4B7320F61EE29C3", // Duel.ini
            "38F3655B98432FF1601316C42F44A0E049DF730A", // FastOptions.ini
            "575B300FBE5A5900B9D072AF73B0CE5FC0680E1C", // Fortress.ini
            "DB0CE24FD8FE75E5E7C03649D04D17E14483A24D", // GlobalCode.ini
            "06EE9314FBD8614B17419C3DD901519700582667", // MultiplayerOptions.ini
            "20F25C4466A140D7B40277DEB9FB5E1CE04B249F", // Newbie Practice.ini
            "23DDBB82E150F9168AC1078EC25338CCAC4E8229", // NoAirflow.ini
            "575B300FBE5A5900B9D072AF73B0CE5FC0680E1C", // Standard.ini
            "D0320F3AAE2382065B8A4E5023489D3489D65A9D"  // Survival Battle.ini
        };

        public static readonly string[] filesToCheck =
        {
            ClientCore.ClientConfiguration.Instance.MPMapsIniPath,
            "Resources/GameOptions.ini",

            "INI/Game Options/Auto Handle.ini",
            "INI/Game Options/Auto Repair.ini",
            "INI/Game Options/Ion Storm.ini",
            "INI/Game Options/Destroyable Bridges.ini",
            "INI/Game Options/No Epic.ini",
            "INI/Game Options/Low Detail.ini",
            "INI/Game Options/No Muzzle.ini",
            "INI/Game Options/No Rain.ini",
            "INI/Game Options/No Rush.ini",
            "INI/Game Options/No Silos.ini",
            "INI/Game Options/No TEU Lift.ini",
            "INI/Game Options/No Tiberium Lifeforms.ini",
            "INI/Game Options/No Turret.ini",
            "INI/Game Options/OreDouble.ini",
            "INI/Game Options/OreQuintuple.ini",
            "INI/Game Options/OreStandard.ini",
            "INI/Game Options/OreTriple.ini",
            "INI/Game Options/Reveal Map.ini",
            "INI/Game Options/Second Tech.ini",
            "INI/Game Options/Unrebuildable Tech.ini",

            "INI/Map Code/Challenge Easy.ini",
            "INI/Map Code/Challenge Hard.ini",
            "INI/Map Code/Challenge Medium.ini",
            "INI/Map Code/Co-Op Easy.ini",
            "INI/Map Code/Co-Op Hard.ini",
            "INI/Map Code/Co-Op Medium.ini",
            "INI/Map Code/Crazy Crates.ini",
            "INI/Map Code/Difficulty Tier.ini",
            "INI/Map Code/Duel.ini",
            "INI/Map Code/FastOptions.ini",
            "INI/Map Code/Fortress.ini",
            "INI/Map Code/GlobalCode.ini",
            "INI/Map Code/MultiplayerOptions.ini",
            "INI/Map Code/Newbie Practice.ini",
            "INI/Map Code/NoAirflow.ini",
            "INI/Map Code/Standard.ini",
            "INI/Map Code/Survival Battle.ini",
        };
    }
}
