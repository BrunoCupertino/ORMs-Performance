using ORMs.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dapper;
using DapperExtensions;

namespace ORMs
{
    class Program
    {
        static void Main(string[] args)
        {
            var times = new List<QueryTime>();

            var qt3 = EFComAsNoTracking();
            ShowQueryTime(qt3);

            var qt1 = EFSqlQuery();
            ShowQueryTime(qt1);

            var qt5 = EFSemAsNoTracking();
            ShowQueryTime(qt5);

            var qt4 = AdoNet();
            ShowQueryTime(qt4);

            var qt2 = Dapper();
            ShowQueryTime(qt2);

            Console.WriteLine(Environment.NewLine + "Ordered:");

            times.Add(qt1);
            times.Add(qt2);
            times.Add(qt3);
            times.Add(qt4);
            times.Add(qt5);

            times.OrderBy(t => t.Time).ToList().ForEach(qt => ShowQueryTime(qt));

            //PrintQuery("select ID, Sigla from TiposLancamento");
          
            Console.ReadKey();
        }

        static void ShowQueryTime(QueryTime qt)
        {
            Console.WriteLine(string.Format("{0}: - Quantidade: {1} - Tempo: {2:00}:{3:00}:{4:00}.{5:00}", qt.Name.PadLeft(20), qt.Count, qt.Time.Hours, qt.Time.Minutes, qt.Time.Seconds, qt.Time.Milliseconds / 10));
        }

        static QueryTime EFSemAsNoTracking()
        {
            var stopWatch = new Stopwatch();
            List<Entidade> entidades;

            stopWatch.Start();

            using (var efContext = new DataContext())
            {
                entidades = efContext.Lancamentos.ToList();

                stopWatch.Stop();
            }            

            var ts = stopWatch.Elapsed;

            return new QueryTime("EFSemAsNoTracking", ts, entidades.Count());
        }

        static QueryTime EFComAsNoTracking()
        {
            var stopWatch = new Stopwatch();
            List<Entidade> entidades;

            stopWatch.Start();

            using (var efContext = new DataContext())
            {
                entidades = efContext.Lancamentos.AsNoTracking().ToList();

                stopWatch.Stop();
            }

            var ts = stopWatch.Elapsed;

            return new QueryTime("EFComAsNoTracking", ts, entidades.Count());
        }

        static QueryTime EFSqlQuery()
        {
            var stopWatch = new Stopwatch();
            List<Entidade> entidades;

            stopWatch.Start();

            using (var efContext = new DataContext())
            {
                entidades = efContext.Database.SqlQuery<Entidade>(string.Format("select ID from {0}", Entidade.TABLE_NAME)).ToList();

                stopWatch.Stop();
            }

            var ts = stopWatch.Elapsed;

            return new QueryTime("EFSqlQuery", ts, entidades.Count());
        }

        static QueryTime AdoNet()
        {
            var stopWatch = new Stopwatch();
            List<Entidade> entidades = new List<Entidade>();

            stopWatch.Start();

            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ToString()))
            {
                var sqlCmd = new SqlCommand(string.Format("select ID from {0}", Entidade.TABLE_NAME), cnn);

                sqlCmd.CommandType = System.Data.CommandType.Text;

                cnn.Open();

                using (var rdr = sqlCmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var entidade = new Entidade
                        {
                            ID = Convert.ToInt32(rdr["ID"])
                        };

                        entidades.Add(entidade);
                    }

                    stopWatch.Stop();
                }
            }

            var ts = stopWatch.Elapsed;

            return new QueryTime("ADO.NET", ts, entidades.Count());
        }

        static void PrintQuery(string query)
        {
            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ToString()))
            {
                var sqlCmd = new SqlCommand(query, cnn);

                sqlCmd.CommandType = System.Data.CommandType.Text;

                cnn.Open();

                using (var rdr = sqlCmd.ExecuteReader())
                {
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        Console.Write(string.Format("{0} -- ", rdr.GetName(i)));
                    }

                    Console.WriteLine();

                    while (rdr.Read())
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            Console.Write(string.Format("{0} -- ", rdr.GetValue(i)));
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        static QueryTime Dapper()
        {
            var stopWatch = new Stopwatch();
            IEnumerable<Entidade> entidades;

            stopWatch.Start();

            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Connection"].ToString()))
            {
                cnn.Open();

                entidades = cnn.Query<Entidade>(string.Format("select ID from {0}", Entidade.TABLE_NAME));
                //entidades = cnn.GetList<Entidade>();

                stopWatch.Stop();
            }

            var ts = stopWatch.Elapsed;

            return new QueryTime("Dapper", ts, entidades.Count());
        }
    }

    public class Entidade
    {
        public const string TABLE_NAME = "Log";

        public int ID { get; set; }
    }

    public class QueryTime
    {
        public QueryTime(string name, TimeSpan time, int count)
        {
            this.Name = name;
            this.Time = time;
            this.Count = count;
        }

        public string Name { get; set; }
        public TimeSpan Time { get; set; }
        public int Count { get; set; }
    }
}
