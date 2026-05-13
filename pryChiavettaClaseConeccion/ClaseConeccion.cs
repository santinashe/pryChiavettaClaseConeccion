using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace pryChiavettaClaseConeccion
{
   
    public static class ClaseConeccion
    {
        // Construye la cadena de conexión según la extensión (usa el ejemplo proporcionado)
        public static string BuildConnectionString(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();//extiende, convierte entra  
            switch (ext)
            {
                case ".accdb":
                    return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=False;";
                case ".mdb":
                    return $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Info=False;";
                default:
                    throw new NotSupportedException($"Extensión de archivo no soportada: {ext}");
            }
        }
        
        // Obtiene nombres de tablas (solo TABLE) de la base de datos
        public static List<string> GetTableNames(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Ruta inválida", nameof(path));//usamos el trow

                      
                throw new FileNotFoundException("El archivo de base de datos no existe.", path);

            var tables = new List<string>();
            var connString = BuildConnectionString(path);

            using (var conn = new OleDbConnection(connString))
            {
                conn.Open();
                // Obtener esquema de tablas

                //CantColumnas cantidad de columnas
                var CantColumnas = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (CantColumnas != null)
                {
                    foreach (DataRow row in CantColumnas.Rows)
                    {
                        var tableType = row["TABLE_TYPE"].ToString();
                        if (tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            var tableName = row["TABLE_NAME"].ToString();
                            tables.Add(tableName);
                        }
                    }
                }
            }

            return tables;
        }

        // Obtiene todos los registros de la tabla indicada

        //datatable cantidad de columnas
        
        public static DataTable GetTableData(string path, string tableName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Ruta inválida", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("El archivo de base de datos no existe.", path);

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Nombre de tabla inválido", nameof(tableName));

            var connString = BuildConnectionString(path);
            var dt = new DataTable();

            using (var conn = new OleDbConnection(connString))
            using (var cmd = conn.CreateCommand())
            using (var adapter = new OleDbDataAdapter(cmd))
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM [{tableName}]";
                adapter.Fill(dt);
            }

            return dt;
        }
    }
}
