﻿using System;
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
            if (isFull)
            {
                string selectSQL = "SELECT Word, Alphagram, isNew, Probability, Playability, Definition FROM WordsToMetadata";
                SqlConnection con = new SqlConnection(connectionString.ToString());
                SqlCommand cmd = new SqlCommand(selectSQL, con);
                //SqlDataReader reader;
                DataSet dsWTA = new DataSet();
                var nameOfTable = "WordsToMetadata";

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                // Try to open database and read information.
                try
                {
                    con.Open();

                    adapter.Fill(dsWTA, nameOfTable);
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error reading table.");
                    Console.WriteLine(err.Message);
                }
                finally
                {
                    con.Close();
                }

                foreach (DataRow row in dsWTA.Tables[nameOfTable].Rows)
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
                    SqlConnection con = new SqlConnection(connectionString.ToString());
                    SqlCommand cmd = new SqlCommand(selectSQL, con);
                    //SqlDataReader reader;
                    DataSet dsWTA = new DataSet();
                    var nameOfTable = "WordsToAlphagrams";

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    // Try to open database and read information.
                    try
                    {
                        con.Open();

                        adapter.Fill(dsWTA, nameOfTable);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("Error reading table.");
                        Console.WriteLine(err.Message);
                    }
                    finally
                    {
                        con.Close();
                    }
                    foreach (DataRow row in dsWTA.Tables[nameOfTable].Rows)
                    {
                        WordsToAlphagrams.Add(row["Word"].ToString(), row["Alphagram"].ToString());
                    }


                }
            }
        }
    }
}