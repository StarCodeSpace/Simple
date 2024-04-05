using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Simple.Dapper;
using Simple.Utils.Helper;
using Simple.WinUI.Forms;

namespace Simple.Tool
{
    internal static class Program
    {
        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        private static void Main()
        {
            ConfigHelper.Init(new string[] { "appsettings.json" }, false, true);

            DbComponent.RegistFactoty("Microsoft.Data.Sqlite", Microsoft.Data.Sqlite.SqliteFactory.Instance);
            DbComponent.RegistFactoty("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
            DbComponent.RegistFactoty("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new BootstrapConsole(MenuTree()) { Text = "Simple ������" });
        }

        /// <summary>�˵���ʼ��</summary>
        /// <returns></returns>
        private static TreeNode[] MenuTree()
        {
            var treeNods = new TreeNode[] {
                new TreeNode
                {
                    Name = "0",
                    Tag = "CodeGeneratorPage",
                    Text = "��������"
                },
                new TreeNode
                {
                    Name = "1",
                    Tag = "WebToolPage",
                    Text = "����վ��"
                }
            };
            return treeNods;
        }
    }
}