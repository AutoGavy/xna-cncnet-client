using Localization;

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
            "INI/Map Code/Difficulty Medium.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Hell.ini"
        };

        public static readonly string[] filesHashArrayCamp =
        {
            "DAD4D370FCF48F08809058CB5300843442348B31", // Hell Settings
            "2D22C883B0C6FEB012811FA2D45A63127F7798F5", // Hard Settings
            "D63191F931C1FEA555E43B13131F2DFB6DE4C9FB", // Normal Settings
            "A0A68A2C59FE030DE427E701EC5811812DCA1A03", // Easy Settings
            "C71C34380B7AEC596B8EED889538C50B07FE6EED", // GlobalCode

            "8AAD50B140D2DDD4E1E49CA64492C3E4A7C0F6C3", // Train 01
            "tttttttttttttttttttttttttttttt", // GDO 01
            "tttttttttttttttttttttttttttttt", // GDO 02
            "tttttttttttttttttttttttttttttt", // GDO 03
            "tttttttttttttttttttttttttttttt", // GDO 04
            "tttttttttttttttttttttttttttttt", // GDO 05
            "tttttttttttttttttttttttttttttt", // GDO 06
            "tttttttttttttttttttttttttttttt", // GDO 07
            "tttttttttttttttttttttttttttttt", // GDO 08
            "tttttttttttttttttttttttttttttt", // GDO 09
            "tttttttttttttttttttttttttttttt", // GDO 10
            "tttttttttttttttttttttttttttttt", // GDO 11
            "tttttttttttttttttttttttttttttt", // GDO 12

            "115BF7C325662D2FAE9AAFFEF07B48FA2B0683BE", // GDI 01
            "4D9F17E4D3FE212469EF30681AED22B0A55D4CEA", // GDI 02
            "002FCB20E2E5A7B131B51835AE7CC8EA024B1AF2", // GDI 03
            "459E56532C271317C154C8FF1E8311465AACDE29", // GDI 04
            "B6DACE5C38801741DCD7419921F6B2531C1D487C", // GDI 05
            "A6C243453D43636C390388A62FB3A98ABD82195E", // GDI 06
            "E508FBB7838E39EC36DC490AF58105E9FF3D4F49", // GDI 07
            "34902BA603F3CE50D52FEEB0DB5465E788A81D9E", // GDI 08

            "0B14E96F9F38989EB804B123287961D162860710", // End 01
            "61FF1C4549223DA96ABBD4FD53CB5493C42061A8", // End 02
            "C0E64B2AF809E2B9ECDDBF886267E6C3B037E443", // End 03
            "F4A6A7BA08D3EE5F8DEB412F80DCD94416C270D0", // End 04
            "A8BC9E677A904434E966C6E60F44129EE4A59343", // End 05
            "2C888AC8ADDF6A5BD7D4CA7DCF11A32301914B53", // End 06
            "2C51E3829DCCBDF1BF48102AED65F641E6F7F215", // End 07
            "01694E76E5D2FDC284EB62C1FAE81A909E746129", // End 08
            "E6F96C43C5FF8870FB7057764DADCC99F4113F14", // End 08 Off
            "E49D70719290B484DE90B92EB9C2377F69D7849B", // End 08 Def
            "92620A88A5D197223254881B1F836ED282AC23CD", // End 08 Sup

            "B1CBE8F768F06E9A23F39637612DD2C2CAA78187", // Nod 01
            "33325CC089E85DAA9267D022506CC9A4C836CC9C", // Nod 02
            "387299AAC98EAE5D977BAF81CFD8B56E4FBEC9FF", // Nod 03
            "EA3EF0BEF7CAF5C0386C2ABB003E7D35FDF29F26", // Nod 04
            "00AE5B459D5159A472ED35A387F0BA132B7B0703", // Nod 05
            "0E67F4A3299E7BE88B94CD8F00168C8AEC504D94", // Nod 06
            "B04CE4D9C3C062CC0C16A93FAEBCD7985E06B0D0", // Nod 07
            "237717F0E6D3D4142D9170F61CD4FA14661DABAA", // Nod 08

            "F14CDD8372547122041C73BF81FB0F07E361003E", // Scrin 01
            "8624FDCE7BC10108761F660F46F7BEDB7EE8FB1B", // Scrin 02
            "6962F17CE16670482D5A1EBB0C866CBF9E528848", // Scrin 03
            "301488643E12C2B3203136B1FD2A1860D0B4E136"  // Scrin 04
        };

        public static readonly string[] filesToCheckCamp =
        {
            "INI/Map Code/Difficulty Hell.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Normal.ini",
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/GlobalCode.ini",

            "MapsTC/Missions/tra01.map",
            "MapsTC/Missions/gdo01.map",
            "MapsTC/Missions/gdo02.map",
            "MapsTC/Missions/gdo03.map",
            "MapsTC/Missions/gdo04.map",
            "MapsTC/Missions/gdo05.map",
            "MapsTC/Missions/gdo06.map",
            "MapsTC/Missions/gdo07.map",
            "MapsTC/Missions/gdo08.map",
            "MapsTC/Missions/gdo09.map",
            "MapsTC/Missions/gdo10.map",
            "MapsTC/Missions/gdo11.map",
            "MapsTC/Missions/gdo12.map",

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
            "MapsTC/Missions/end03.map",
            "MapsTC/Missions/end04.map",
            "MapsTC/Missions/end05.map",
            "MapsTC/Missions/end06.map",
            "MapsTC/Missions/end07.map",
            "MapsTC/Missions/end08.map",
            "MapsTC/Missions/end08_off.map",
            "MapsTC/Missions/end08_def.map",
            "MapsTC/Missions/end08_sup.map",

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
            "613FF3426100BEF37DD90C441E0A44741A08076F", // Maps Settings
            "9B09D4ABE3147A0D6AE0BABC63718DCB5A2A133B", // GameOptions

            "851713637974DB9E0C582291D7E594EC529F4637", // Challenge Hard
            "C949F5472A78B40850F4C509DB27737338857888", // Challenge Medium
            "F1F1AD90AEECAAA2D645FC2E6756574AEB3854F2", // Challenge Easy
            "1F7826758AD4510D34A3307FAFA061DE7166F58B", // Co-Op Hard
            "111ED2C9103A495366827008A7AF1C5F67424392", // Co-Op Medium
            "4EC8B63AB756F8F324F137DFE12F42BDA916E71B", // Co-Op Easy
            "E42E4B2478B7CFF648DCCD5BEAC8C8B1C7122E25", // Crazy Crates
            "0C1F20E6C905EE8FA6439DD16F7F9CC238662A0F", // Duel
            "AA3CE317C698327F8F871A4F3E4E336861B50135", // Newbie Practice
            "575B300FBE5A5900B9D072AF73B0CE5FC0680E1C", // Standard
            "894ADFC5FD6152DF7465A4F974674A7627720EBA", // Difficulty Tier 1
            "6489B8214E0E3A8683C6A89E2FB5F939462521AC", // Difficulty Tier 2
            "D0320F3AAE2382065B8A4E5023489D3489D65A9D", // Survival Battle
            "715869652AFC3D5E7C0102CB573CE586FD1174ED", // Auto Repair
            "C71C34380B7AEC596B8EED889538C50B07FE6EED", // GlobalCode
            "73697101B8F5B765ABE2752F7C17C7DAAC58C1F3", // FastOptions
            "F3209DF66955D249933608AAF931F5263E187372", // MultiplayerOptions

            "3FDFE5A78683E1440F174F55E1EB826D39563571", // Destroyable Bridges
            "3BAD7FEFA37B15628E411C9C0A79570E88A251EC", // No Rain
            "7D8BB86A7FA28123FFB416A28E5BBDB4CB61B74E", // No Silos
            "6C327D16DC521804BA9DB5C2C8E2F85964FFAE2A", // No TEU Lift
            "3B305AE3A6E803A82DF6506B84D10787A9D97BB5", // No Tiberium Lifeforms
            "5C655472A5BBEF36843D50544D26E36AA2400295", // Low Detail
            "1126195ECD2DA0CD10F025BD417C251FB0105F86", // Reveal Map
            "7DE809012A6CF96503B78FD9C83EA4E0E9E62470", // Second Tech
            "2340D596F788A50F31BA459A0141451174315D60", // No Muzzle
            "C600B33BCCE62DD46D74CC40267D602D867550A6", // Unrebuildable Tech

            "CBED59CD3177715734C9C0A079710CC0EE26B8FE", // Ruins
            "0007550A6608AA69A0B61F47F50C92B3C7D670B2", // GDI 05
            "F986B82DB862FF2038AD7B9A0A95D4EDC820C46A", // GDI 07
            "353DCA4D9EF44C69CC26EDE82A1F88FF8B66FEA2", // GDI 08
            "AE492749859B1777B322B9ACD807417B081764DE", // Nod 03
            "F7213DC6780945971FFF6866A4148272DFB91399", // Nod 08
            "22BB8939AD0A32C331C2E662D1E1BE2A6C85481F", // Scrin 04

            "4A521F6EEEE836AEE0EC1E51B979691FD6110699", // Co-Op GDI Exit
            "127BC46D25639B35CAC72D3949DCC9326D83F94F", // Co-Op Nod Silence
            "A0B88F62B5EA9335ED6B4E11C839B266AE2B9A85"  // Co-Op Nod Plunder
        };

        public static readonly string[] filesToCheck =
        {
            ClientCore.ClientConfiguration.Instance.MPMapsIniPath,
            "Resources/GameOptions.ini",

            "INI/Map Code/Challenge Hard.ini",
            "INI/Map Code/Challenge Medium.ini",
            "INI/Map Code/Challenge Easy.ini",
            "INI/Map Code/Co-Op Hard.ini",
            "INI/Map Code/Co-Op Medium.ini",
            "INI/Map Code/Co-Op Easy.ini",
            "INI/Map Code/Crazy Crates.ini",
            "INI/Map Code/Duel.ini",
            "INI/Map Code/Newbie Practice.ini",
            "INI/Map Code/Standard.ini",
            "INI/Map Code/Difficulty Tier 1.ini",
            "INI/Map Code/Difficulty Tier 2.ini",
            "INI/Map Code/Survival Battle.ini",
            "INI/Map Code/Auto Repair.ini",
            "INI/Map Code/GlobalCode.ini",
            "INI/Map Code/FastOptions.ini",
            "INI/Map Code/MultiplayerOptions.ini",

            "INI/Game Options/Destroyable Bridges.ini",
            "INI/Game Options/No Rain.ini",
            "INI/Game Options/No Silos.ini",
            "INI/Game Options/No TEU Lift.ini",
            "INI/Game Options/No Tiberium Lifeforms.ini",
            "INI/Game Options/Low Detail.ini",
            "INI/Game Options/Reveal Map.ini",
            "INI/Game Options/Second Tech.ini",
            "INI/Game Options/No Muzzle.ini",
            "INI/Game Options/Unrebuildable Tech.ini",

            "MapsTC/Challenge/c_ruins.map",
            "MapsTC/Challenge/c_gdi05.map",
            "MapsTC/Challenge/c_gdi07.map",
            "MapsTC/Challenge/c_gdi08.map",
            "MapsTC/Challenge/c_nod03.map",
            "MapsTC/Challenge/c_nod08.map",
            "MapsTC/Challenge/c_scr04.map",

            "MapsTC/Cooperative/coop_gexit.map",
            "MapsTC/Cooperative/coop_nsilence.map",
            "MapsTC/Cooperative/coop_nplunder.map"
        };
    }
}
