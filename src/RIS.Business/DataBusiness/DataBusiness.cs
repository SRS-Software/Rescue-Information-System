﻿#region

using System.Data.Entity;
using System.IO;
using System.Linq;
using RIS.Data;
using RIS.Data.Migrations;

#endregion

namespace RIS.Business
{
    public partial class DataBusiness : IBusiness
    {
        public void CheckConnection()
        {
            using (var _databaseContext = new DatabaseContext())
            {
                //Migrate database V6 to V7
                var _oldDatabasePath = @"C:\ProgramData\RIS\RISv6-DB.sdf";
                if (File.Exists(_oldDatabasePath) && !File.Exists(_databaseContext.Database.Connection.Database))
                {
                    //Copy old db to new name for backup
                    File.Copy(_oldDatabasePath, _databaseContext.Database.Connection.Database);
                    //Delete __MigrationHistory table
                    _databaseContext.Database.ExecuteSqlCommand(@"DELETE FROM [__MigrationHistory];");
                    //Add migration script 201401251404348_InitialCreate because db already exists
                    _databaseContext.Database.ExecuteSqlCommand(
                        @"INSERT INTO [__MigrationHistory] ([MigrationId],[ContextKey],[Model],[ProductVersion]) VALUES (N'201401251404348_InitialCreate',N'RIS.Data.Migrations.Configuration',0x1F8B0800000000000400ED1DDB6EE3B8F5BD40FFC1F063316B279901DA06CE2E3CC96411EC6632886707455F02C56612A1B2E44AF23469D12FEB433FA9BF504AA2285E0EAFBAD899F14B10F372786E3C220FCF21FFF79FFFCE7E7A5E47A3AF28CDC2243E1B1F4F8EC623142F9355183F9E8DB7F9C30F7F1AFFF4E3EF7F37FBB05A3F8FBED4EDDE16ED70CF383B1B3FE5F9E6743ACD964F681D649375B84C932C79C827CB643D0D56C9F4E4E8E8CFD3E3E329C220C618D66834BBDDC679B846E50FFCF33C899768936F83E83A59A12823E5B86651421D7D0CD628DB044B7436BEBD5A4C2E823CB80F32341ECDA330C0382C50F4301E05719CE4418E313CFD2D438B3C4DE2C7C5061704D1E7970DC2ED1E82A8E855627EDA34B725E2E8A42062DA74AC412DB7599EAC1D011EBF255C998ADDBD783BA65CC37CFB80F99BBF145497BC3B1BCF830473791556E0C5014FCFA3B468CCB377C2767A3362ABDE505D389EBC9B1CBF3B3A99FCF1CDE87C1BE5DB149DC5689BA741F466F4697B1F85CB5FD0CBE7E46F283E8BB751C4A28911C5755C012EFA94261B94E62FB7E881207F75311E4DF97E53B123EDC6F4A948BA8AF3B727E3D1473C78701F21AA050CF98B3C49D1CF28466990A3D5A720CF518A8578B542251FA5D185B18ABFF56858EDF0DC198FAE83E75F51FC983F9D8DDF1D1DE1E972193EA3555D4450F82D0EF15CC3BDF2748BA4513E065FC3C7124161BCF9FC261B8F6E515456664FE1A69A02135C71C788F9324DD6B74954F5682AEE3E07E923CA31C60954BB48B6E952C066366D344AAF67F31B7BF59ADF1CB46A70AD1246F9F0BC49519695FAB2271A5C55F2CA5B9451CD64F5B6ACA815DA76F8F3647D1FC6C4D6BA6201CD1E0E3D7B2CE84CF59FCB104FE4996E8BD217F4142E23A4904D9090FA3BFC2F871257237388AF6E6760282C97CF18E9723037EAB13E25195135FD88D653BC5091365A246936AC648EAA0DE244E10A739E2D97B49AAB6CA5D3AE0A7DD0E63DF9785EAEB35F501C6F0BE87D8F751D84F10D9D9DEF13AC83416C9E9F0094CFE8391F06DFF324DAAE458B62EE769BFCC3A9CF65F0DC9E3318C8308C996F36780606F9BE2CB9AC8C9F688C41CB688D4A14A4EB60B3F9394DB61BC55A836D72D72C4B98AF05D8425E7BC0CDDAAD415898F6CB10B6D7C176ABC72A1974B5EA7F260EF491F0B24E9E4B74DF69232DB6F4B3CB16D9BF7EF97005635AD4DC8986A04113A896A636D4A6D5BC2E005A4FE7A2F16116ABC71A6C61F41EFD1385CBA76106FB9486495A52DF6EB581F534EEC3C2B5F9E67A4C49D16CE8A6AD35A6D70B8571BB5EDC1183C2D8B3BA50FEF2D39A761FFBEB85FD27FE7A713009EAB186F693FF9661E683AA54D4DC558AD6A8122D9454A9A971F5F1A93F80066D963EC8929E7B69734189B53A178D0FFA3CB83ECB3BE268BE2A9CF4AF6FB1AAB4E58609286ABF3C35BDB4FF328C10FE82C7B9FB59A8DCF53033069F194A3DA3D281B58D56C3272A40B5F40980DAB45A575080EE2A78D0BCC1356FC8535340B7879C4BFA63498FA9241A73DD74B345B2EC5F2C65001449DD1D6B141A0CE55A69AE034D5A4D758AACE54427ED0FD35C3D1661D1601EBCF36413168E34A7B35119CCC7EDFA1EA5370FB543ADFDB1C52DDA2469DE057A15A4BE30BC08B33C8897A80B1C3BC70D4FB1AE60DD26DB1C5DAD8347D429C40B942DD370534C9D0E8EBA9AB56C075A5D03EB40737CD777CE165FFC24693E0ABE8B3B0C6EBE74DD62D04E07DBAF1E6B079B8B5CE548AAEAEE6A49730BA1A602DA4D30B56DF7112E6B8BAAF941BDD463356BFBE321371227FD0F7691BC470F98378BA726C0A2031FCF52B759B0991DC00601983BF6D315452B0D3A65FD1D9DD422525CB562E2F26D3A98BE0467A7395CF6394C64F558FBF79D70523D7856C0EA69A37AF32C4B9661895813125445FDF0047E88572355E475C54C129F8CD989552B2CC299F07867E3A3C9E4586217008D4E2A2DB43F8864310498E96ABC133A740027850B4E1A78108D5C0E8E89752EF4F221B54A0415F1B51C863416D5817045F4B7A3AE3810CC85ACA9D082E3D71AA414941A94180E09F660A18B7CE1A81EA53C0C213E0CB27C209B83C80DC177462EFB33038A5250A1A90D596870ACE27EECA9D70627F971D78101CDD1990A3FE01CADC1AA3A01B62716381567482C82233A53737A22AE543BF978DC0517052C802E0B8D70200CF299ABD0D23AD01B0499D3237B92B5E75C00EC7EBE5680B34685B0CE73D3E04BDDEB4E865C770EE0CE67376D6036451A59413B240E33806883F821978408B2DEAD75287268F5ABC752B114167125FB2627C16BB7798EFC55F2A05A82E309940721DEBFD41B70B24F2BCACB5C0369C387AD2DD9F36564FB219251C05DA01CC8EBC63B8A66E50FAE39259E08C0CA0C001946B17A3375A5CB1E0516741D6000A481620B420C6194D1E13FCC067024484B82527D234CB894212E3206C5B7CAD09544AF497DAB0FB8A1B36CC3215090A5B705AC85670443AC2F04849A733326D45A299061AC99153035184B00A555514321E64A00C598106E2E928C1CA65A4C4F167D349A9D3BC5B79EE6538BCEB5593474E6AC206E65491EA374308D8AD592C52EDE955A793DC442E06D6C7BDA850C588078CD86DE664BCF23DF98541D17E04D7CF792E7D3CD64DAD57B7BF3EE9EC1D68A6870336FC73C1FB92B927300F95B6CF85DB6FC2C49C2C751A712FA4DBE05B33D98042621C81C327A01ACFD000C19E44BAF61896EE76FCD640FAE30E1B2322F140E01834B804197AC343474CB4E0096DA12AFF6D3A30988076604EC1B3078070C481AFC0126CDF0A0118C9E93A9353A0CAC5D060C0DECCA4DC3079D930082D6E9A7110AE490D963F220D8FA1018729AF5A886351A9781159FFD14863D41055545E54DB0F027F0681B19007B102420744DDE11F9C2B1998A091AFF82B58741A2A55ED11BB9023B14CC0CD670A53EC9A36E045A379B5637E39182D9547185DEEC1A7F85C2F891B9528F948C16D57D7AE73F2CDCAF9B5B5730A64B8ECDA2D3838E942769F08884DA22D078852EC334CB9BFBFCCE576BA999E83451ECC3EAD120BF882CBB7A7B56F72AFE274177AA1BF044280D2F2F31796B14E725A548B17390BB8F8AFB0D03BC5400CEC8AB9B3354E7ECBADED5A937DBBF2A9121CCA602FE229BA6129F04CD15996F271A78616F2B91F98D8F20A05EBBE6BF0A029B5CC0C261CBF7479ACC16A9CD34AB9D7B5E934CD9B91F11375765B1309AD2BD114E7BC9F88B656099B49F76EC654E2C1CB6DC1E5A7D5D130BA92E738352DD4A24C2A94ADD20D5D73189B0EA723768E52D4D22A8B2D081E755BA29C7EEAAC80986CC225A680F87BB978985C555ECCDDC363A3F6C6D2F7718E2617EF5FDFB99EDF48A2016042D1CD46A3868F08E3445E537B15490F29CCB5D2FE06EFDA80368B73D8C3677950C0B8BAB705828D0DB62B885022DB58744EE8B61C190A2BD51338507D2D60C5D2F7C8C0FD46BD70B8C1DF1BFF2E27A0BA03C56769700DC6DD722D0AC5B9ABB3484B54B53F14D597FC851EBAD2440C080BBCAD800D9B502ED5A585DC8A88D68F64F22DD7A4B9413BABC6C819BCFA11C72B4430DA10715DEFA51C7D7B86B87B2674F6E1636C75F584035154EC6BA4E78160C765DEC040BCA7716E0424DECC7E053FB59D07C8D2B4413E2EA56AE233549FF32FCA6CE15AA1EEF56F8928B00006C498D2B44FE3A00192E5FEF055DB81A403186D0CACD2272970588C691ABF4806B9C47BA867B6397F993CF361FEF2668D1EBFBADE9BEEB4FF84E45D35A28DEE2184A105CE23ABC1492E2E2EDE09DA8E04957F3EBE0F1E9E72C40BE66CF14874400B4D49E2AF4D7578514BDBFF509CD474648C7C82486547D524C1A589C0617E11CF283095C74A94CBD15EF99B7A6EE2039143CA0433B6045224C3CB172C684EED547575991B34F93E5ED6915235DBC44CEC4CD69E4CEB4F217BE14FFE6AD01359C8EE42F85F9ED9912A8C96DAF0242ECBC31A0A06A661F3300B11C0C8DF7E47901A10B350003FFF74A0F604A5B6B009F3FA00F5B50D97F17E94389069E7CAE4175207F287361AFA4AFA6B5BD0D50E451D81C7033CDB507D90ED6419B7AE16B2538A05DD80B6D2EC7A0FADC5AFE608A88E6F05A6A0B1D556BC31800866A524A3CB95942EC40D49A1C951D68636B6933A92F9A934BD2023A9D04CE8801AE4909329EBC2AE174204529E3C65776D78BA125C624F2A80FFBEB16C0893E143902993431DD67300E19D06927300F33D05A62605A92E97851B7C9529E23028CD3241CEDC3864B935DB5570BAE5EB75D505E96FE78916BDA463DD4F95B9EECAF0176A01BEA84B0BD520D35C55D580E363F4DEB02A5ADECDCE4F05C94F3CF7CCDFEB2430B21A7D6ED95022869ED48FE42829ED911DEB46DA90B60169F27EF2B589D29049810B8576AA122D8AC155266A2D8849E0C9012FA9B662692AC402E5DB16447917C58B22123198A629A60D5643CC2B47F0D57458AE0E225CBD1BAD4B4C9E2EFD102A55F517A8E26EF26474DB3EB200E1F50965797DE8ECBBA7914065995444AB2204FC52B99ACD2228FDF16699168B59E8ADDDD932B0B2859B6E2AEE4059E2554A6160E71796F79C58FF17A5EC79704AA53A66A80F86B902E9F8214B8B1B7812A5FCF6BFFAAA3780AF03D734D062ABF6FD41968F18C88618D741BDA55BC42CF67E37F955D4F47577FB9E37BBF19DDA478669F8E8E46FF76C7835D2FBB62D1F4B5C7C15E3915FEE957ABA34D6A243388238CFA38C14D5255AF367AC23A39DDC66E7AF6A023DF9682F462C4D8C4CD8E41D7999C15D8FBD05D9D9B1CCE1E50AB133AC1D9660BE4B67924C20302F790A80F83680667C7FCE1B239075A6CA8CF165EED8CA5C9961D8BA71F4BE0A68CD682953DD5AF569E7D594A2E73B263D84D22A5BF99215994031902F128EAD5AACB907B3539C5F07B661BF8B1161F08DF7BCB69CA01FC9E25ECF184F481774AA03DBA322E9997993BF48F0CED97003FACF4C0C80D83A6670F7B5E30FFF0D56A3BF0847197669B7FDBD5C7E3A27DAFD867AD05BD55EC8398E9A5627FDCC4578AFDB1EB142BEE7D627F38D0DBC42DA181EF127B6DF7E13789BDF4D6FC2271B72B1945CADDABB54C03AF61BE1D930E3C1DDBC33AE6A473D8D0C3B03E939889337074D32F3B58B534C7D96E83D7FD7A58B128B3F75EAD8A0F7A486B8CC8B763A345FCB2DC4997C06092813CA0E37410FAAB34D34A627D1E60B5759EB6F68CABA365DDD5C02C573A9A1B1F49B75652DCA14A3939EA3C5D9CEAB079408E60C0B6597A740C37CE916EEDA44750769459D9AB0F498151F696C654152E6F3D392DECE730ECDAE104EFF691EEF60F735711707A28F2DB9258D4282D161241749EC4599E06A17C0740B1155B869B2062F015DA582E7E0AB6516862CD05DAA0B858D50844D90C65CED1A7D0054D3671A0FB57CBB9071D598139BD2EEEAA415DC95EFF9E421F4A60A900FA8C9121C46FF9887BFBD9CE3CC6C9EB8FF5DBEF7B3DEF756F09009F33653EFE00327779C7BE83B7EB77257917790C2B7D7D06F310B3DE3E7B5CF5329FFD9BF3C7525AC54D7C812294A351E580C18630C896C14A5E2715B90656D8800A0636E8C7C8186EB3EF5EE12C6E0DF0754AEC5001A174153F9BF41D28DD90F6CD57DD766EE96C2F4C105EDBE4645A150CA25FB2774A44446B6A3BD22CC5DB0B9DAB95F1060C47E7DBAE14CA60B6F6EBBBB91F2A36F817D34FD7F6E473A9BF2644783197936755308862D57E5871F8B2AC1725523C1ED1B9EE28EE737174290FAB28269B24080A10D237A326B6A21B504BD4B7BF0CB1F8D65E61B3174A529F1408A3432BB1D7A5248A8B86F659490C966497CBE5E1D564A8A5B18B9EEC76416C7C351E7AE51D3E4C80AA9D5C8D4D683A08B6279DB07A9EA8730DD13DB8B397470DC0154D2A39D621D7AC1469D92BD008F563373B5603FD2D54C3580BE64E268DD49AE85641704C85AB26882A5517F56515746F6AF4A108B62AA7BD846A3025E0AF64D28BAD8A65946547CA5F8122A85E62D8A51EE8AE9EEA5C0D84AB9BE87749B8664952047247577370CA2C0E9A6816301CA1BAB2E96CBCBA2FEE6CAFA262D816D2CB41F270C539B73C4A510A022F2FFE36C1640E3E210268A502FDE62E69C340EA51B44358C317FC84322D7C3D488E7047B261C46A612F0D541543F0C975AF26428A7DAD8C7E510A225D541840567E370966550C01ADDE5135818556C8D22050236848E88D4E6B0474E31A86338F42D77AD218B4061A81BE52684105B38C800861AA15B430EF6ED90DA71C4837842D70F221548C406AD5C390D7869C66BCC68881CD8CF3DFDAF0008714A04DB030405233F3E8D45909CE6EA5D5A0956663A43073B44661946063C75FAA2886778E98EAE6EB068406F17E97E6B3485FB791D71040A8A7BE8F7803A425218C9983A951EECB95118E2C9E5C859EC83E0814DE940128D484F0B5939AB45011DF31694F1EFF648A4C9C3A568D4315C25381E4E024EA83AD2081BABCEDA13E6766E9E16B740CD15878F57B25BD3189F89EED99045F78DB85A6EC88319A0016802BF6EF83481E7C8694AA40C30CE50758F1B0493F6C502B87FD63241D4F9FE11923850600EC30BD32220510303857051A9285E510F7004A97E4A9A56D7AB4443CD164252B23392071D2412C64E70CEF8DB4204E5869720FA174499CC6881B5E2F6965A4FA250F7C9044A6D0783A663C1F63B087AA350C90F6EAC22B18ED9900BDBA21F3C074F0031FFD3028D3B29D122B3C2701CA5A75B6A139DDE0B1652AF4C48A3CAA8B3A2253782F4145ACC687AFF6E24B5893F29E09965E0FA075B369B5A32605F8A7F44AC06C7ABBC5BDD755DAE4EC0265E16303A278F820464BCEE54DDB5CC50F49ED7B1730AA9B8857EBA13C580579304FF3F02158E6B87A595CC2515C66F92588B6B8C987F53D5A5DC537DB7CB3CD31C9687D1FBDB0CC283CF8BAF1675309E7D94D79A34ED6050918CD1093806EE2F7DBB0102EC1FB12484A5580288E06C81512852CF3E22A89C7170AE963125B0222ECA3271A9FD17A136160D94DBC08BE221FDCF01AE057F4182C5F3E91671ED440CC82E0D93EBB0883C73458670446D31FFFC43ABC5A3FFFF87F6BE5783544EB0000,N'6.1.0-30225');");

                    //Enalbe and execute migration to latest version 
                    Database.SetInitializer(new MigrateDatabaseToLatestVersion<DatabaseContext, Configuration>());
                    _databaseContext.Database.Initialize(true);

                    //Reseed all tables
                    if (_databaseContext.Aaos.Count() > 0)
                    {
                        var _maxIdAaos = _databaseContext.Aaos.Max(f => f.Id);
                        if (_databaseContext.Aaos.Count() > _maxIdAaos) _maxIdAaos = _databaseContext.Aaos.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Aaos] ALTER COLUMN [Id] IDENTITY ({_maxIdAaos + 1}, 1)");
                    }

                    if (_databaseContext.AlarmappGroups.Count() > 0)
                    {
                        var _maxIdAlarmappGroups = _databaseContext.AlarmappGroups.Max(f => f.Id);
                        if (_databaseContext.AlarmappGroups.Count() > _maxIdAlarmappGroups)
                            _maxIdAlarmappGroups = _databaseContext.AlarmappGroups.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [AlarmappGroups] ALTER COLUMN [Id] IDENTITY ({_maxIdAlarmappGroups + 1}, 1)");
                    }

                    if (_databaseContext.Amss.Count() > 0)
                    {
                        var _maxIdAms = _databaseContext.Amss.Max(f => f.Id);
                        if (_databaseContext.Amss.Count() > _maxIdAms) _maxIdAms = _databaseContext.Amss.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Ams] ALTER COLUMN [Id] IDENTITY ({_maxIdAms + 1}, 1)");
                    }

                    if (_databaseContext.Fileprints.Count() > 0)
                    {
                        var _maxIdFileprints = _databaseContext.Fileprints.Max(f => f.Id);
                        if (_databaseContext.Fileprints.Count() > _maxIdFileprints)
                            _maxIdFileprints = _databaseContext.Fileprints.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Fileprints] ALTER COLUMN [Id] IDENTITY ({_maxIdFileprints + 1}, 1)");
                    }

                    if (_databaseContext.Filters.Count() > 0)
                    {
                        var _maxIdFilters = _databaseContext.Filters.Max(f => f.Id);
                        if (_databaseContext.Filters.Count() > _maxIdFilters)
                            _maxIdFilters = _databaseContext.Filters.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Filters] ALTER COLUMN [Id] IDENTITY ({_maxIdFilters + 1}, 1)");
                    }

                    if (_databaseContext.Pagers.Count() > 0)
                    {
                        var _maxIdPagers = _databaseContext.Pagers.Max(f => f.Id);
                        if (_databaseContext.Pagers.Count() > _maxIdPagers)
                            _maxIdPagers = _databaseContext.Pagers.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Pagers] ALTER COLUMN [Id] IDENTITY ({_maxIdPagers + 1}, 1)");
                    }

                    if (_databaseContext.Printers.Count() > 0)
                    {
                        var _maxIdPrinters = _databaseContext.Printers.Max(f => f.Id);
                        if (_databaseContext.Printers.Count() > _maxIdPrinters)
                            _maxIdPrinters = _databaseContext.Printers.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Printers] ALTER COLUMN [Id] IDENTITY ({_maxIdPrinters + 1}, 1)");
                    }

                    if (_databaseContext.Users.Count() > 0)
                    {
                        var _maxIdUsers = _databaseContext.Users.Max(f => f.Id);
                        if (_databaseContext.Users.Count() > _maxIdUsers) _maxIdUsers = _databaseContext.Users.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Users] ALTER COLUMN [Id] IDENTITY ({_maxIdUsers + 1}, 1)");
                    }

                    if (_databaseContext.Vehicles.Count() > 0)
                    {
                        var _maxIdVehicles = _databaseContext.Vehicles.Max(f => f.Id);
                        if (_databaseContext.Vehicles.Count() > _maxIdVehicles)
                            _maxIdVehicles = _databaseContext.Vehicles.Count();

                        _databaseContext.Database.ExecuteSqlCommand(
                            $"ALTER TABLE [Vehicles] ALTER COLUMN [Id] IDENTITY ({_maxIdVehicles + 1}, 1)");
                    }
                }
                else
                {
                    //Enalbe migration to latest version 
                    Database.SetInitializer(new MigrateDatabaseToLatestVersion<DatabaseContext, Configuration>());
                }

                //Check Connection
                _databaseContext.CheckConnection();
            }
        }
    }
}