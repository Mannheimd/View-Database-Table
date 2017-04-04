using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View_Database_Table
{
    public partial class MainWindow : Window
    {
        private DataTable databaseName_Table = new DataTable();
        private DataTable tableName_Table = new DataTable();
        private DataTable output_Table = new DataTable();

        public MainWindow()
        {
            InitializeComponent();
            loadDatabases();
        }

        private void loadDatabases()
        {
            try
            {
                // Clear the Databases DataTable
                databaseName_Table.Clear();

                // Connect to ACT7 instance
                string connectionString = @"Data Source=(local); Initial Catalog=master; Server=(local)\ACT7; Integrated Security=True;";
                SqlConnection dataConnection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand("select name from sys.databases where name != 'master' and name != 'model' and name != 'msdb' and name != 'tempdb' and name != 'ActEmailMessageStore'", dataConnection);
                dataConnection.Open();

                // Create data adaptor and populate database list
                SqlDataAdapter dataAdaptor = new SqlDataAdapter(command);
                dataAdaptor.Fill(databaseName_Table);

                // Close the connection and get rid of the data adaptor
                dataConnection.Close();
                dataAdaptor.Dispose();

                List<string> databases = new List<string>();

                foreach (DataRow dr in databaseName_Table.Rows)
                {
                    databases.Add(dr["name"].ToString());
                }

                databases.Sort();
                databaseNameList.ItemsSource = databases;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void databaseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                try
                {
                    string selectedDatabase = (sender as ListBox).SelectedItem.ToString();

                    // Clear the TableName DataTable
                    tableName_Table.Clear();

                    // Connect to ACT7 instance
                    string connectionString = @"Data Source=(local); Initial Catalog=" + selectedDatabase + @"; Server=(local)\ACT7; Integrated Security=True;";
                    SqlConnection dataConnection = new SqlConnection(connectionString);
                    SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM " + selectedDatabase + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", dataConnection);
                    dataConnection.Open();

                    // Create data adaptor and populate table name list
                    SqlDataAdapter dataAdaptor = new SqlDataAdapter(command);
                    dataAdaptor.Fill(tableName_Table);

                    // Close the connection and get rid of the data adaptor
                    dataConnection.Close();
                    dataAdaptor.Dispose();

                    List<string> tables = new List<string>();

                    foreach (DataRow dr in tableName_Table.Rows)
                    {
                        tables.Add(dr["TABLE_NAME"].ToString());
                    }

                    tables.Sort();
                    tableNameList.ItemsSource = tables;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            if (tableNameList.SelectedItems.Count == 1 & databaseNameList.SelectedItems.Count == 1)
            {
                try
                {
                    string selectedTable = tableNameList.SelectedItem.ToString();
                    string selectedDatabase = databaseNameList.SelectedItem.ToString();

                    // Clear the output DataTable and the DataGrid
                    output_Table = new DataTable();
                    outputDataGrid.ItemsSource = null;

                    // Connect to ACT7 instance
                    string connectionString = @"Data Source=(local); Initial Catalog=" + selectedDatabase + @"; Server=(local)\ACT7; Integrated Security=True;";
                    SqlConnection dataConnection = new SqlConnection(connectionString);
                    SqlCommand command = new SqlCommand("SELECT * FROM " + selectedDatabase + ".dbo." + selectedTable, dataConnection);
                    dataConnection.Open();

                    // Create data adaptor and populate table name list
                    SqlDataAdapter dataAdaptor = new SqlDataAdapter(command);
                    dataAdaptor.Fill(output_Table);

                    // Close the connection and get rid of the data adaptor
                    dataConnection.Close();
                    dataAdaptor.Dispose();

                    outputDataGrid.ItemsSource = output_Table.DefaultView;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            loadDatabases();
            tableNameList.ItemsSource = null;
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            if (outputDataGrid.ItemsSource != null)
            {
                try
                {
                    string exportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\SQLTableExport.csv";

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < output_Table.Columns.Count; i++)
                    {
                        string data = output_Table.Columns[i].ToString().Replace("\"", "\"\"");
                        sb.Append("\"" + data + "\"");
                        if (i < output_Table.Columns.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();
                    foreach (DataRow dr in output_Table.Rows)
                    {
                        for (int i = 0; i < output_Table.Columns.Count; i++)
                        {
                            string data = dr[i].ToString().Replace("\"", "\"\"");
                            sb.Append("\"" + data + "\"");

                            if (i < output_Table.Columns.Count - 1)
                                sb.Append(",");
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(exportPath, sb.ToString());
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
                finally
                {
                    MessageBox.Show("File saved to desktop");
                }
            }
        }
    }
}
