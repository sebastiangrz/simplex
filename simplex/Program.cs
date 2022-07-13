using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace simplex
{
    class Program
    {
        static void Main(string[] args)
        {
            // Welocoming and Reading filepath String
            Console.WriteLine("Starting Simplex Solver");
            Console.WriteLine("Using filepath: " + args[0]);
            string filepath = args[0];

            var globalStopwatch = new Stopwatch();
            var simplexStopwatch = new Stopwatch();

            globalStopwatch.Start();

            // Getting the objective function and constraints from file
            List<float> objectiveFunction = getObjectiveFunction(filepath);
            List<List<float>> constraints = getConstraints(filepath);

            // converting the objective function and constraints to a transposed table and adding slack vars
            List<List<float>> tableWithSlacks = getTransposedTableWithSlacks(objectiveFunction, constraints);

            simplexStopwatch.Start();
            float result = simplex(tableWithSlacks);
            simplexStopwatch.Stop();

            globalStopwatch.Stop();
            // Print the result
            Console.WriteLine("Final result: " + result * -1);
            Console.WriteLine("Time needed overall: " + globalStopwatch.ElapsedMilliseconds);
            Console.WriteLine("Time needed for simplex algorithm: " + simplexStopwatch.ElapsedMilliseconds);
        }

        static List<float> getObjectiveFunction(string filepath)
        {
            // Read the function as a string
            string function;
            using (var sr = new StreamReader(filepath))
            {
                for (float i = 0; i < 1; i++)
                    sr.ReadLine();
                function = sr.ReadLine();
            }

            List<float> objectiveFunctionList = new List<float>();
            
            /* Following iterates through the string of the objective function.
             * If it finds a seperator (like + or -) it puts every digit to the
             * temp variable. If an "x" is found it converts the value of the
             * temp var to a float and adds it to the objective function list.
             * After that it puts itself in the search for seperator mode again.
            */
            bool searchSeperator = true;
            string temp = "";
            for(int i = 0; i < function.Length; i++)
            {
                if (searchSeperator)
                {
                    temp = function[i] == '-' ? "-" : "";
                    searchSeperator = !(function[i] == '+' || function[i] == '-');
                }
                else if (function[i] == 'x')
                {
                    searchSeperator = true;
                    objectiveFunctionList.Add(float.Parse(temp));
                }
                else if (Char.IsDigit(function[i]))
                    temp += function[i];
            }
            // Adds a 0 to have a result like in the constraints.
            objectiveFunctionList.Add(0); 

            Console.WriteLine("Read objective function:");
            for (int i = 0; i < objectiveFunctionList.Count; i++)
            {
                if (i == objectiveFunctionList.Count - 1)
                    Console.Write("Result: " + objectiveFunctionList[i]);
                else
                    Console.Write(objectiveFunctionList[i] + "*x" + i + "\t");
            }
            Console.WriteLine();
            Console.WriteLine();
            return objectiveFunctionList;
        }

        static List<List<float>> getConstraints(string filepath)
        {
            // Read Constraints as strings
            var lineCount = File.ReadAllLines(filepath).Length;
            List<string> constraints = new List<string>();
            using (var sr = new StreamReader(filepath))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var temp = sr.ReadLine();
                    if (i >= 3)
                        constraints.Add(temp);
                    
                }
            }

            /* Following iterates through the string of every constraint.
             * If it finds a seperator (like + or -) it puts every digit to the
             * temp variable. If an "x" or ";" is found it converts the value of
             * the temp var to a float and adds it to the objective function
             * list. After that it puts itself in the search for seperator mode
             * again. */
            List<List<float>> constraintsList = new List<List<float>>();
            foreach (string constraint in constraints)
            {
                
                    List<float> constraintList = new List<float>();
                    bool searchSeperator = true;
                    string tempInt = "";
                    for (int i = 0; i < constraint.Length; i++)
                    {
                        if (searchSeperator)
                        {
                        tempInt = constraint[i] == '-' ? "-" : "";
                        searchSeperator = !(constraint[i] == '+' || constraint[i] == '-' || constraint[i] == '=') ;
                        }
                        else if (constraint[i] == 'x' || constraint[i] == ';')
                        {
                            searchSeperator = true;
                            constraintList.Add(float.Parse(tempInt));
                        } 
                        else if (Char.IsDigit(constraint[i]))
                            tempInt += constraint[i];
                    }
                constraintsList.Add(constraintList);
            }

            Console.WriteLine("Read constraints:");
            foreach (List<float> constraint in constraintsList)
            {
                for (int i = 0; i < constraint.Count; i++)
                {
                    
                    if (i == constraint.Count)
                        Console.Write("Result: " + constraint[i]);
                    else
                        Console.Write(constraint[i] + "*x" + i + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            return constraintsList;

        }

        static List<List<float>> getTransposedTableWithSlacks(List<float> objectiveFunction, List<List<float>> constraints)
        {
            constraints.Add(objectiveFunction);
            List<List<float>> transposedTable = new List<List<float>>();

            /* Following iteration transposes the constraints and the objective
             * function to create a table-like matrix */
            for(int i = 0; i < constraints[0].Count; i++)
            {
                List<float> temp = new List<float>();
                for (int j = 0; j < constraints.Count; j++)
                {
                    temp.Add(constraints[j][i]);
                }
                transposedTable.Add(temp);
            }

            Console.WriteLine("Transposed constraints and obejctive function:");
            foreach (List<float> constraint in transposedTable)
            {
                for (int i = 0; i < constraint.Count; i++)
                {
                    if (i == constraint.Count - 1)
                        Console.Write("Result: " + constraint[i]);
                    else
                        Console.Write(constraint[i] + "*x" + i + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();


            // After getting the new table add the slack vars.
            List<float> slacks = new List<float>();
            for(int i = 0; i < transposedTable.Count - 1; i++)
                slacks.Add(0);
            for (int i = 0; i < transposedTable.Count; i++)
            {
                int temp = transposedTable[i].Count - 1;
                transposedTable[i].InsertRange(transposedTable[i].Count - 1, slacks);
                if (i != transposedTable.Count - 1)
                    transposedTable[i][temp + i] = 1;
            }

            Console.WriteLine("Table with slacks:");
            foreach (List<float> constraint in transposedTable)
            {
                for (int i = 0; i < constraint.Count; i++)
                {
                    if (i == constraint.Count - 1)
                        Console.Write("Result: " + constraint[i]);
                    else if (i >  constraint.Count - slacks.Count - 2)
                        Console.Write(constraint[i] + "*s" + (i - (constraints.Count - 1)) + "\t");
                    else
                        Console.Write(constraint[i] + "*x" + i + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            
            return transposedTable;
        }

        static float simplex(List<List<float>> table)
        {
            bool finished = false;
            int iterationCounter = 0;
            while(finished == false)
            {
                int[] pivotElement = getPivotElement(table);
                int pivotRow = pivotElement[0];
                int pivotColumn = pivotElement[1];
                float pivotElementValue = table[pivotRow][pivotColumn];

                /* Get the pivot element to 1 and do the same division to all 
                 * other elements of the row */
                for(int i = 0; i < table[pivotRow].Count; i++)
                {
                    if(table[pivotRow][i] != 0)
                        table[pivotRow][i] = table[pivotRow][i] / pivotElementValue;
                }

                /* Get all other elements in pivot column to 0 and do the same 
                 * subtraction to all other elements in each column */
                for(int i = 0; i < table.Count; i++)
                {
                    // Pivot element needs to stay 1
                    if(i != pivotRow)
                    {
                        float multiplicator = table[i][pivotColumn];
                        for(int j = 0; j < table[i].Count; j++)
                        {
                            table[i][j] = table[i][j] - (multiplicator * table[pivotRow][j]);
                        }
                        
                    }
                }

                iterationCounter++;
                Console.WriteLine("Iteration " + iterationCounter + ": ");
                Console.WriteLine();
                for (int i = 0; i < table.Count; i++)
                {
                    for (int j = 0; j < table[i].Count; j++)
                    {
                        if (j == table[i].Count - 1)
                            Console.Write("Result: " + table[i][j]);
                        else if (j > table[i].Count - table.Count -1 )
                            Console.Write(table[i][j] + "*s" + (j - (table.Count - 3)) + "\t");
                        else
                            Console.Write(table[i][j] + "*x" + j + "\t");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();


                /* Stop the simplex when all elements in the objective function
                 * are smaller than or 0 */
                if (!table[table.Count - 1].Exists(e => e > 0))
                    finished = true;
            }
            return table[table.Count -1][table[table.Count -1].Count -1];
        }

        static int[] getPivotElement(List<List<float>> table)
        {
            int[] pivotElement = new int[2];

            /* Get pivot column by iterating through every element in the 
             * objective function, and finding the biggest element which is not
             * zero */
            List<float> objectiveFunction = table[table.Count - 1];
            int pivotColumn = 0;
            for(int i = 0; i < objectiveFunction.Count - 1; i++)
            {
                if (objectiveFunction[i] != 0)
                    if (objectiveFunction[i] >= objectiveFunction[pivotColumn])
                        pivotColumn = i;
            }
            pivotElement[1] = pivotColumn;

            /* Divide all results of the constraints with the  elements in the 
             * pivot column of the constraints and store it in a list to keeo 
             * the indexes correct.
             * Only divides when both elements arent 0 */
            List<float> dividedPivotColumnResult = new List<float>();
            for(int i = 0; i < table.Count -1; i++)
            {
                if (table[i][pivotColumn] != 0 && table[i][pivotColumn] != 0)
                    dividedPivotColumnResult.Add(table[i][table[i].Count -1] / table[i][pivotColumn]);
                else
                    dividedPivotColumnResult.Add(0);
            }

            /* Iterates through all results of the previous divisions and finds 
             * the lowest element bigger than 0. The index of this element is 
             * the pivot row. */
            int pivotRow = 0;
            for(int i = 0; i < dividedPivotColumnResult.Count; i++)
            {
                if (dividedPivotColumnResult[i] > 0)
                    if (dividedPivotColumnResult[pivotRow] <= 0)
                        pivotRow = i;
                    else if (dividedPivotColumnResult[i] <= dividedPivotColumnResult[pivotRow])
                        pivotRow = i;
            }
            pivotElement[0] = pivotRow;
            Console.WriteLine("Found pivot element: " + table[pivotElement[0]][pivotElement[1]] + " at row " + pivotElement[0] + " and column " + pivotElement[1]);
            Console.WriteLine("");
            Console.WriteLine("*********************************************");
            return pivotElement;
        }
    }
}
