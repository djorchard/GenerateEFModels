using Dan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Reflection;


namespace GenerateEFModels
{
    public class ParseDbml
    {
        private List<Table> _tables = new();


        public void Parse(string dbml)
        {
            //split the code into rows
            string[] rows = dbml.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            //which table are we currently parsing?
            Table? currentTable = null;

            //loop through each row
            for (var index = 0; index < rows.Length; index++)
            {
                string row = rows[index];

                //remove comments
                if (row.Contains("//"))
                {
                    row = row.Before("//");
                }

                //trim whitespace
                row = row.Trim();

                //skip empty rows
                if (row == "")
                {
                    continue;
                }

                //check if it is the end of the table
                if (row.StartsWith("}"))
                {
                    currentTable = null;
                    continue;
                }

                //check if it is the start of a table
                if (row.StartsWith("Table"))
                {
                    //get the table name
                    //eg. Table User {
                    row = row.Between("Table ", "{").Trim().Singularize(false);

                    //generate a new table object
                    Table table = new();

                    //set the current table reference
                    currentTable = table;

                    //ignore table settings (bit inside [])
                    if (row.Contains('[') && row.Contains(']'))
                    {
                        row = row.Before("[") + row.After("]").Trim();
                    }

                    //check if there is an alias
                    if (row.Contains(' '))
                    {
                        Log.Debug($"'{row}' has a space in it");
                        if (row.Contains("as"))
                        {
                            table.Alias = row.After("as").Trim();
                        }
                        row = row.Before(" ");
                    }

                    table.Name = row;

                    //add to list of tables
                    _tables.Add(table);
                    continue;
                }

                if (row.StartsWith("Ref:"))
                {
                    //TODO: Relationship (ignore for now until i can be bothered to parse them)
                }

                //must be a column name
                if (currentTable != null)
                {
                    //get type and column name:
                    //address varchar(255) [settings]
                    if (row.Contains(' '))
                    {
                        //replaces spaces inside of quotes with _
                        while (row.Contains('"'))
                        {
                            row = row.Before("\"") + row.Between("\"", "\"").Replace(" ", "_") + row.After("\"",null,false,true);
                        }

                        string columnName = row.Before(" ");

                        Column column = new()
                        {
                            Name = columnName
                        };

                        row = row.After(" ").Trim(); //remove column name from row text to continue parsing

                        //get settings in []
                        if (row.Contains('[') && row.Contains(']'))
                        {
                            ParseColumnSettings(column, row.Between("[", "]"));

                            //remove settings from row text to continue parsing
                            row = row.Before("[") + row.After("]").Trim();
                        }

                        //only text left should be the type

                        column.Type = row.Trim();

                        //finally add the column to the table
                        currentTable.Columns.Add(column);
                    }
                }

            }


            //generate a list of relationships

            //debug:
            foreach (Table table in _tables)
            {
                Log.Debug(table.Name);
                foreach (Column column in table.Columns)
                {
                    Log.Debug($"  {column.Name} {column.Type} {column.Note} {column.IsPrimaryKey} {column.IsUnique} {column.IsNullable} {column.AutoIncrement} {column.DefaultValue}");
                }
            }

            GenerateModels();
        }

        public void GenerateModels()
        {
            string tempFolder = Path.Combine(App.Folder ,"Temp");

            //make temp folder
            if (!Path.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            
            //delete old models
            string modelsFolder = Path.Combine(tempFolder, "Models");

            if (Path.Exists(modelsFolder))
            {
                Directory.Delete(modelsFolder, true);
            }
            //create new empty models folder
            Directory.CreateDirectory(modelsFolder);

            //generate a model for each table
            StringBuilder dataContextContent = new();
            dataContextContent.Append("""
            // <auto-generated> This file was generated by Dan's Generate EF Models tool
            #nullable disable
            using Microsoft.EntityFrameworkCore;

            namespace Data;

            public partial class DataContext : DbContext
            {
                public DataContext(DbContextOptions<DataContext> options) : base(options) { }

            """);


            foreach (Table table in _tables)
            {
                //add table to data context
                dataContextContent.AppendLine($"\tpublic virtual DbSet<{table.Name}> {table.Name.Pluralize()} {{ get; set; }}");

                //generate the model
                string model = GenerateModel(table);

                //save the model
                string file = Path.Combine(modelsFolder, table.Name + ".cs");
                File.WriteAllText(file, model);
            }

            dataContextContent.AppendLine("}");

            string dataContextFileName = Path.Combine(modelsFolder, "DataContext.cs");
            File.WriteAllText(dataContextFileName, dataContextContent.ToString());

            //open Windows Explorer to show the models
            Process.Start("explorer.exe",modelsFolder);

        }

        private string GenerateModel(Table table)
        {
            //convert table in to C# Entity Framework Model
            //start with the using statements
            StringBuilder sb = new();
            sb.AppendLine("// <auto-generated> This file was generated by Dan's Generate EF Models tool");
            sb.AppendLine("#nullable disable");
            sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("");
            sb.AppendLine("namespace Models");
            sb.AppendLine("");

            //generate indexes for unique values
            foreach (Column column in table.Columns.Where(x => x.IsUnique))
            {
                sb.AppendLine($"[Index(nameof({column.Name}), IsUnique = true)]");
            }

            //generate the model class
            sb.AppendLine($"public partial class {table.Name}");
            sb.AppendLine("{");

            //generate the properties
            foreach (Column column in table.Columns)
            {
                if (!string.IsNullOrWhiteSpace(column.Note))
                {
                    sb.AppendLine($"\t//{column.Note}");
                }
                if (column.IsPrimaryKey)
                {
                    sb.AppendLine("\t[Key]");
                }
                if (!column.IsNullable)
                {
                    sb.AppendLine("\t[Required]");
                }
                var line = $"\tpublic {column.Type} {column.Name} {{get; set; }}";
                if (column.DefaultValue != null)
                {
                    //convert ' to " for strings
                    line += $" = {column.DefaultValue.Replace('\'','"')};";
                }
                sb.AppendLine(line);
            }
            sb.AppendLine("}");
            return sb.ToString();

        }

        private void ParseColumnSettings(Column column, string settingsText)
        {
            string[] settings = settingsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string setting in settings)
            {
                string settingText = setting.Trim();
                if (settingText.StartsWith("Note"))
                {
                    column.Note = settingText.After("note:").Trim();
                }
                else if (settingText.Equals("Primary Key", StringComparison.CurrentCultureIgnoreCase) || settingText.Equals("pk", StringComparison.CurrentCultureIgnoreCase))
                {
                    column.IsPrimaryKey = true;
                }
                else if (settingText.Equals("not null", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.IsNullable = false;
                }
                else if (settingText.Equals("unique", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.IsUnique = true;
                }
                else if (settingText.Equals("increment", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.AutoIncrement = true;
                }

                else if (settingText.StartsWith("default:", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.DefaultValue = settingText.After("default:").Trim();
                }

                else
                {
                    Log.Debug($"Unknown column setting: {settingText}");
                }
            }
        }
    }
}

class Table
{
    public string Name;
    public List<Column> Columns = new();
    public string? Alias;
}

class Column
{
    public string Name;
    public string Type;
    public string? Note;
    public bool IsPrimaryKey;
    public bool IsUnique;
    public bool IsNullable = true;
    public bool AutoIncrement;
    public string? DefaultValue;
}

class Relationship
{
}

