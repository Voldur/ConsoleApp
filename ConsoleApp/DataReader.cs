namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            // Initialize ImportedObjects as a list
            ImportedObjects = new List<ImportedObject>();

            // Use using statement for proper disposal of StreamReader
            using (var streamReader = new StreamReader(fileToImport))
            {
                // Read each line from the file
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (line != null)
                    {
                        // Split the line into values
                        var values = line.Split(';');
                        // Create a new ImportedObject and add it to ImportedObjects list
                        var importedObject = new ImportedObject
                        {
                            Type = values[0],
                            Name = values[1],
                            Schema = values[2],
                            ParentName = values[3],
                            ParentType = values[4],
                            DataType = values[5],
                            IsNullable = values[6]
                        };
                        ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                    }
                }
            }

            // Clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                // Trim and replace unwanted characters for each property
                importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }

            // Assign number of children for each object
            foreach (var importedObject in ImportedObjects)
            {
                // Count children based on ParentType and ParentName
                importedObject.NumberOfChildren = ImportedObjects.Count(io => io.ParentType == importedObject.Type && io.ParentName == importedObject.Name);
            }

            // Iterate through ImportedObjects to print database, tables, and columns
            foreach (var database in ImportedObjects.Where(io => io.Type == "DATABASE"))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                // Print all database's tables
                foreach (var table in ImportedObjects.Where(io => io.ParentType.ToUpper() == database.Type && io.ParentName == database.Name))
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    // Print all table's columns
                    foreach (var column in ImportedObjects.Where(io => io.ParentType.ToUpper() == table.Type && io.ParentName == table.Name))
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }

            Console.ReadLine();
        }
    }

    // Changed properties access modifiers to public
    class ImportedObject : ImportedObjectBaseClass
    {
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public int NumberOfChildren { get; set; } // Renamed field to adhere to naming conventions
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}