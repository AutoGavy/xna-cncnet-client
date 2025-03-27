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
            "2FB0BF27A34BE9A108C4C3618BFE03F798C2B44B", // Difficulty Easy.ini
            "EC69C905090090157FD85EED71BEF8D9F7D93883", // Difficulty Hard.ini
            "CBF46AF0B95C95EBE3F4CC496C0286AB043B27BB", // Difficulty Hell.ini
            "49E81D8F7CA06AB99380179A78E7743D8E9E63E5", // Difficulty Normal.ini

            "5D4C1678629A05F96F768E10A20F4D315D5535CB", // Train 00
            "E592792971A75BB794B6865635AF0CA9D6173504", // Train 01
            "102694875CF4C1CE0D0260452AAEAB658FE464AE", // Train 02
            "13F32DD5FC7E9724F26339CA604AEB1D2B7DF1F8", // Train 03

            "6773B65378B0D2A803E8C08478B1E99DF0B15B5C", // GDI 01
            "F330A2ED32F6AE7D8096279CFCA24D022FE63943", // GDI 02
            "9511B17B3C8156C5A9F54A47B4B52404FB4AA23C", // GDI 03
            "CEB229380E7A79F3B3535E8186035913FFA9C010", // GDI 04
            "5547337A21DE52B91841837CB3627CA9B74EA08A", // GDI 05
            "8FBAA1FAF9805688886BBCE4D30DF7FD8AF2A746", // GDI 06
            "179A3AC49B349C3FFFCE72C46BFB1D1DF7210C48", // GDI 07
            "02C323FBBCBF903C2996238E92F343CE46BCC13D", // GDI 08

            //"5F1A3B9F7C4F1E94C10269D39E34A33BFD36174E", // End 01
            //"F8C153308738BFFC5B0D3AB7571DF181EA21C773", // End 02

            "B58615C53A70C7520D3E32F8AE60844E4C1AFB34", // Nod 01
            "A7549237519B06ECAD64F50CE7B35867FCA6456A", // Nod 02
            "A250E0533A5311213E01ED74C593490E218EC5D1", // Nod 03
            "8564BD5D30638E969A9EF8CCCE0D1BE489B53DC3", // Nod 04
            "EEB0AA70C38F8992C37812B7FFCBF4592B5AAC6E", // Nod 05
            "49C686B7649B50D13E37E1C940317B3BCDBF2185", // Nod 06
            "275ED29B5860D6B1106F4118E9DCAEA8B217C43E", // Nod 07
            "5D50EA539D2F9A796426B423D1A9601F630BA19D", // Nod 08

            "B54AD6E2AB8BD14E8C6D2DFAEEAFD653FDF474F6", // Scrin 01
            "8274B16C657412C41948797F4DB5E5879CFE441F", // Scrin 02
            "EC1BDF9983FD6280B53A7C6E3DACD0BCFFBD5F90", // Scrin 03
            "619A6AE05623923878C1683F05492CBF3486C009"  // Scrin 04
        };

        public static readonly string[] filesToCheckCamp =
        {
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Hell.ini",
            "INI/Map Code/Difficulty Normal.ini",

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

            //"MapsTC/Missions/end01.map",
            //"MapsTC/Missions/end02.map",

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
            "5EF86EB02F6359668841D9A9FE35F9E588F3EDE9", // GameOptions

            "715869652AFC3D5E7C0102CB573CE586FD1174ED", // Auto Repair
            "587DB8F7C95EB8E8E98B4E86DDCFFCADDC9B3A17", // Capture Protect
            "3FDFE5A78683E1440F174F55E1EB826D39563571", // Destroyable Bridges
            "D5493E148240DF4BF5E19B8F5CE6239AD289436F", // IH Stage 1
            "4DE695C7E2156A78D999F44838D038E4CA07F94B", // IH Stage 2
            "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", // IH Stage Standard
            "67717D5358C517D09C6E36E549C07EE91FB10D96", // Ion Storm
            "302D9B5C7002557CFBB2DB201FA31080C6B543EF", // No Epic
            "B2551F45259872B01093A317E304FF2A82BAC30D", // No Rain
            "DD2844CC99461018917ECF24495F75148C41DAE9", // No Rush
            "DD7B28C0E59A907C25E94CFD2DED42B3E7233A9B", // No Silos
            "43C618F3FDE397270BF7744436D15825B8804A47", // No T4
            "B2B71102BB35EC095CC9C1D038B7A1958B17C0D8", // No TEU Lift
            "3B305AE3A6E803A82DF6506B84D10787A9D97BB5", // No Tiberium Lifeforms
            "3033513D21B7BBC4D6809D2AB263AF27D6C33E9D", // No Turret
            "C08A489553D42F00DDBBF00A8727E031E8A8DAD7", // Ore Stage 1
            "B02F4760D4E0236ECCE12404549635DE104DC9ED", // Ore Stage 2
            "C91E751FAEDED01A8457DE9690ED10596F4D3E15", // Ore Stage 3
            "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709", // Ore Standard
            "0CFBCB6D5428DA579183F121E103AAB2B4F00B4D", // Reveal Map
            "5BE1408AC0AC83C02496D8EB4111FE44715729FA", // Thick Shroud
            "C600B33BCCE62DD46D74CC40267D602D867550A6", // Unrebuildable Tech

            "4168FDE4D78BFBB28E9E3E9813A0F5DAAD92C803", // Challenge Easy.ini
            "84E3268CB68D4012F554785181362C8DF8DD583E", // Challenge Hard.ini
            "5861612BDD162B181E20F3413AD0B0D2C3CDA81E", // Challenge Medium.ini
            "9C4B09D6A575080E8B2EAE8A79DF380C87FB88CA", // Co-Op Easy.ini
            "9E381980FA67419426187F9DC14A5E676C383612", // Co-Op Hard.ini
            "EB0BA8C1B90EF316568F9902B08EFC6CA134F28A", // Co-Op Medium.ini
            "E42E4B2478B7CFF648DCCD5BEAC8C8B1C7122E25", // Crazy Crates.ini
            "C6B244582C611BA12BF79FF1D6E19FF3E21516D1", // Difficulty Tier.ini
            "D42ECB8AE645547E361AF96DB4B7320F61EE29C3", // Duel.ini
            "38F3655B98432FF1601316C42F44A0E049DF730A", // FastOptions.ini
            "575B300FBE5A5900B9D072AF73B0CE5FC0680E1C", // Fortress.ini
            "D31CD1FBE20A061BDF69FDCB17BD2339CD7084B9", // MultiplayerOptions.ini
            "575B300FBE5A5900B9D072AF73B0CE5FC0680E1C", // Standard.ini
            "39A43C1A7E65C794D2364999CBC059FC7F83646E", // Time Rebellion Abyss.ini
            "DF799C5A91E38FBA5BB8E305BB18A6EDA905661E", // Time Rebellion Burtal.ini
            "DD04C65DF53C3F545F5460172F901C402A26FC05", // Time Rebellion Hard.ini
            "C58C1B75936F0756F279F61D4824A4112CCC60C6", // Time Rebellion Normal.ini
            "B4B18A264C0A5B97977A565AF3B363C1ADF58E9F"  // Unlimited Epic.ini
        };

        public static readonly string[] filesToCheck =
        {
            "Resources/GameOptions.ini",

            "INI/Game Options/Auto Repair.ini",
            "INI/Game Options/Capture Protect.ini",
            "INI/Game Options/Destroyable Bridges.ini",
            "INI/Game Options/IHStage1.ini",
            "INI/Game Options/IHStage2.ini",
            "INI/Game Options/IHStandard.ini",
            "INI/Game Options/Ion Storm.ini",
            "INI/Game Options/No Epic.ini",
            "INI/Game Options/No Rain.ini",
            "INI/Game Options/No Rush.ini",
            "INI/Game Options/No Silos.ini",
            "INI/Game Options/No T4.ini",
            "INI/Game Options/No TEU Lift.ini",
            "INI/Game Options/No Tiberium Lifeforms.ini",
            "INI/Game Options/No Turret.ini",
            "INI/Game Options/OreStage1.ini",
            "INI/Game Options/OreStage2.ini",
            "INI/Game Options/OreStage3.ini",
            "INI/Game Options/OreStandard.ini",
            "INI/Game Options/Reveal Map.ini",
            "INI/Game Options/Thick Shroud.ini",
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
            "INI/Map Code/MultiplayerOptions.ini",
            "INI/Map Code/Standard.ini",
            "INI/Map Code/Time Rebellion Abyss.ini",
            "INI/Map Code/Time Rebellion Brutal.ini",
            "INI/Map Code/Time Rebellion Hard.ini",
            "INI/Map Code/Time Rebellion Normal.ini",
            "INI/Map Code/Unlimited Epic.ini"
        };
    }
}
