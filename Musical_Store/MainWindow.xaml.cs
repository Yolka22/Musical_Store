using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Musical_Store
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SqlDataAdapter Adapter = new SqlDataAdapter();
        SqlConnection Connection = new SqlConnection();
        string connection_str = ConfigurationManager.ConnectionStrings["Sql_Server"].ToString();
        DataTable dataTable = new DataTable();
        string sqlQuery = "";
        SqlCommand command = new SqlCommand();

        public MainWindow()
        {
            InitializeComponent();
            Connection = new SqlConnection(connection_str);
            Connection.Open();

            sqlQuery = "SELECT * FROM VinylRecords";
            command = new SqlCommand(sqlQuery, Connection);
            Adapter = new SqlDataAdapter(command);
            Adapter.Fill(dataTable);

            main_grid.ItemsSource = dataTable.DefaultView;

            Connection.Close();

            LoadAvailableRecordsFromDatabase();
            LoadArchivedRecordsFromDatabase();
        }


        private void LoadAvailableRecordsFromDatabase()
        {
            try
            {
                
                Connection.Open();

               
                SqlCommand command = new SqlCommand("SELECT * FROM AvailableRecords", Connection);

                
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                
                dataGridAvailableRecords.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке доступных пластинок: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                
                Connection.Close();
            }
        }

        
        private void LoadArchivedRecordsFromDatabase()
        {
            try
            {
                
                Connection.Open();

                
                SqlCommand command = new SqlCommand("SELECT * FROM ArchivedRecords", Connection);


                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);


                dataGridArchivedRecords.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке архивированных пластинок: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {

                Connection.Close();
            }
        }



        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (main_grid.SelectedItem != null)
            {
                // Получение выделенной строки
                DataRowView selectedRow = (DataRowView)main_grid.SelectedItem;

                // Удаление строки из DataTable
                selectedRow.Row.Delete();
            }
        }
        private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранение изменений из DataSet в базу данных
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(Adapter);
                Adapter.Update(dataTable);

                main_grid.ItemsSource = dataTable.DefaultView;

                MessageBox.Show("Данные успешно обновлены в базе данных.", "Обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SellMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получение выделенной строки
            DataRowView selectedRow = (DataRowView)dataGridAvailableRecords.SelectedItem;

            // Получение данных о пластинке
            int recordID = (int)selectedRow["RecordID"];
            string title = (string)selectedRow["Title"];
            int quantity = (int)selectedRow["Quantity"];

            if (int.TryParse(Count.Text, out int sellQuantity))
            {
                if (sellQuantity > 0 && sellQuantity <= quantity)
                {

                    // Открытие подключения к базе данных
                    Connection.Open();

                    // Создание команды для списания пластинок
                    SqlCommand sellCommand = new SqlCommand("UPDATE AvailableRecords SET Quantity = Quantity - @SellQuantity WHERE RecordID = @RecordID", Connection);
                    sellCommand.Parameters.AddWithValue("@SellQuantity", sellQuantity);
                    sellCommand.Parameters.AddWithValue("@RecordID", recordID);
                    sellCommand.ExecuteNonQuery();

                    // Перенос пластинок в таблицу архивированных пластинок
                    SqlCommand archiveCommand = new SqlCommand("INSERT INTO ArchivedRecords (RecordID, Title, Quantity) VALUES (@RecordID, @Title, @Quantity)", Connection);
                    archiveCommand.Parameters.AddWithValue("@RecordID", recordID);
                    archiveCommand.Parameters.AddWithValue("@Title", title);
                    archiveCommand.Parameters.AddWithValue("@Quantity", sellQuantity);
                    archiveCommand.ExecuteNonQuery();

                    // Обновление данных в DataGrid
                    LoadAvailableRecordsFromDatabase();
                    LoadArchivedRecordsFromDatabase();
                    Connection.Close();
                    }
                }
            }
        private void Genre_Search_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                Connection.Open();


                command = new SqlCommand($@"SELECT Title, Genre FROM VinylRecords where Genre = '{Genre.Text}' ", Connection);


                Adapter = new SqlDataAdapter(command);
                dataTable = new DataTable();
                Adapter.Fill(dataTable);

    
                main_grid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                Connection.Close();
            }

        }
        private void Bande_Search_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                Connection.Open();


                command = new SqlCommand($@"SELECT VinylRecords.Title,Bands.BandName FROM VinylRecords JOIN Bands ON VinylRecords.BandID = Bands.BandID WHERE Bands.BandName = '{Bande.Text}' ", Connection);


                Adapter = new SqlDataAdapter(command);
                dataTable = new DataTable();
                Adapter.Fill(dataTable);


                main_grid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                Connection.Close();
            }

        }
        private void Name_Search_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Connection.Open();


                command = new SqlCommand($@"SELECT Title, Genre FROM VinylRecords where Title = '{Name.Text}' ", Connection);


                Adapter = new SqlDataAdapter(command);
                dataTable = new DataTable();
                Adapter.Fill(dataTable);


                main_grid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                Connection.Close();
            }

        }
    }
    }



