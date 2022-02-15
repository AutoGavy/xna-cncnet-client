namespace DTAClient.DXGUI.Generic
{
    public class InfoShared
    {
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
