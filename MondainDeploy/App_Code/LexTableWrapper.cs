using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MondainDeploy
{
    /// <summary>
    /// Summary description for LexTableWrapper
    /// </summary>
    public class LexTableWrapper
    {
        public Dictionary<string, string> WordsToAlphagrams { get; set; }
        public Dictionary<string, WordData> WordsToMetadata { get; set; }

        public LexTableWrapper(System.Configuration.ConnectionStringSettings connectionString, bool isFull)
        {
            WordsToAlphagrams = new Dictionary<string, string>();
            WordsToMetadata = new Dictionary<string, WordData>();
            // Some of this code can probably be reordered out of the if/else statement.
            // Return to this when I've built tests for DB interaction.
            if (isFull)
            {
                string selectSQL = "SELECT Word, Alphagram, isNew, Probability, Playability, Definition FROM WordsToMetadata";
                SqlConnection sqlCon = new SqlConnection(connectionString.ToString());
                SqlCommand sqlCmd = new SqlCommand(selectSQL, sqlCon);
                //SqlDataReader reader;
                DataSet dataWordsToAlpha = new DataSet();
                var nameOfTable = "WordsToMetadata";

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd);

                // Try to open database and read information.
                try
                {
                    sqlCon.Open();

                    adapter.Fill(dataWordsToAlpha, nameOfTable);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading table.");
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    sqlCon.Close();
                }

                foreach (DataRow row in dataWordsToAlpha.Tables[nameOfTable].Rows)
                {
                    WordsToMetadata.Add(row["Word"].ToString(),
                        new WordData(
                            row["Alphagram"].ToString(),
                            Convert.ToBoolean(row["isNew"]),
                            Convert.ToInt32(row["Probability"]),
                            Convert.ToInt32(row["Playability"]),
                            row["Definition"].ToString()
                            )
                        );
                    WordsToAlphagrams.Add(row["Word"].ToString(), row["Alphagram"].ToString());
                }

            }
            else
            {
                {

                    string selectSQL = "SELECT Word, Alphagram FROM WordsToAlphagrams";
                    SqlConnection sqlCon = new SqlConnection(connectionString.ToString());
                    SqlCommand sqlCmd = new SqlCommand(selectSQL, sqlCon);
                    DataSet dataWordsToAlpha = new DataSet();
                    var nameOfTable = "WordsToAlphagrams";

                    SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd);

                    // Try to open database and read information.
                    try
                    {
                        sqlCon.Open();

                        adapter.Fill(dataWordsToAlpha, nameOfTable);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("Error reading table.");
                        Console.WriteLine(err.Message);
                    }
                    finally
                    {
                        sqlCon.Close();
                    }
                    foreach (DataRow row in dataWordsToAlpha.Tables[nameOfTable].Rows)
                    {
                        WordsToAlphagrams.Add(row["Word"].ToString(), row["Alphagram"].ToString());
                    }


                }
            }
        }
    }
}