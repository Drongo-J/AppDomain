using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Thread.FileIO;

namespace Thread
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //Console.WriteLine(path);

            var sandbox = Helper.CreateSandBox();

            //Utils utils = new Utils();
            var type = typeof(Utils);

            // artiq bu obyekt domainde yarandi ve her istediyini ede bilmez, mehdudiyyetleri var
            // isi bitdikden sonta onun vaxtinda evvel ramdan silir 
            var utils = sandbox.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName) as Utils;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\New\a.txt";
            Console.WriteLine(utils.GetFileText(path));

            AppDomain.Unload(sandbox);
        }
    }

    public static class Helper
    {
        public static AppDomain CreateSandBox()
        {
            // AppDomain yaranmamisdan qabaq yazilan kod
            Contract.Ensures(Contract.Result<AppDomain>() != null);

            var platform = Assembly.GetExecutingAssembly();

            var name = platform.FullName+": Sanbox" + Guid.NewGuid();

            // setup elave permissionlar yazam ucundur
            var setup = new AppDomainSetup()
            {
                ApplicationBase = Path.GetDirectoryName(platform.Location)
            };

            PermissionSet permissionSet = new PermissionSet(System.Security.Permissions.PermissionState.None);

            string testFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\New";

            // read icazeni bagladiqda Utils.GetFileText read ede bilmir (mehdudiyyet icra olunur)
            permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Write /* | FileIOPermissionAccess.Read*/ , testFolder));

            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var sandbox = AppDomain.CreateDomain(name, null, setup, permissionSet);
            Contract.Assert(sandbox != null);
            return sandbox;
        }
    }

    namespace FileIO
    {
        public class Utils : MarshalByRefObject
        {
            public string GetFileText(string textFileName)
            {
                return File.ReadAllText(textFileName);
            }
        }
    }
}
