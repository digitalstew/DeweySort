using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace DeweySort_v1._0
{
    // DeweySort project and code provided as open source,
    // free of all restrictions or support.

    class DeweyDataManager
    {
        // loads data from file 
        // provides data to the program
        // provides minimal error checking

        private string dataFile = "";
        private bool dataAvailable = false;
        private string[] DeweyData;
        private string errMessage = "";


        public DeweyDataManager(string pathFilename)
        {
            // constructor
            dataFile = pathFilename; 
        }

        public void initializeData()
        {
            // see if data file exists
            bool fileExists = File.Exists(dataFile);
            if (!fileExists)
            {
                dataAvailable = false;
                errMessage = "No data file found";
            }
            else  // file exists
            {
                loadDeweyData();
            }
        }

        public bool dataIsAvailable()
        {
            // used by outside callers
            return dataAvailable;
        }

        public string errorMessage()
        {
            // used by outside callers
            return errMessage;
        }

        // used for testing
        //public int itemCount()
        //{
        //    return DeweyData.Length;
        //}

        private bool loadDeweyData()
        {
            // first determine the item count in the file
            if (getFileItemCount())
            {
                // data array has been initialized, now load it
                if (loadFileItems())
                {
                    dataAvailable = true;
                }
                else
                {
                    dataAvailable = false;
                }
            }
            else
            {
                dataAvailable = false;
            }

            return dataAvailable;
        }

        private bool getFileItemCount()
        {
            // determine the number of data lines in the file
            // so the data array can be initialized to the correct size
            int count = 0;
            string line;

            try
            {
                TextReader reader = new StreamReader(dataFile);
                while ((line = reader.ReadLine()) != null)
                {
                    // ignore the comment lines
                    if (!line.StartsWith("//"))
                    {
                        count++;
                    }
                }
                reader.Close();

                if (count > 0)
                {
                    DeweyData = new string[count];
                    return true;
                }
                else
                {
                    errMessage = "Data Missing Error: ";
                    errMessage += "No data found in file";
                    dataAvailable = false;
                    return false;
                }
            }
            catch (Exception e)
            {
                errMessage = "Data File Error: ";
                errMessage += e.Message;
                dataAvailable = false;
                return false;
            }
        }

        private bool loadFileItems()
        {
            string line;
            int index = 0;

            try
            {
                // load the data, ignore the comment lines in the data file
                TextReader reader = new StreamReader(dataFile);
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("//"))
                    {
                        DeweyData[index] = line;
                        index++;
                    }
                }
                reader.Close();
                return true;
            }
            catch (Exception e)
            {
                errMessage = "Data File Error: ";
                errMessage += e.Message;
                dataAvailable = false;
                return false;
            }
        }

        public void getNarrowListing(ref string[] inputArray)
        {
            // used by outside caller to pull narrow range of data
            if (inputArray == null || inputArray.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Array error encountered in getNarrowListing");
            }
            else
            {
                int seed = Environment.TickCount;
                Random rnd = new Random(seed);
                int ri = rnd.Next(0, DeweyData.Length);

                // make sure random starting point is within the data range
                if (ri + inputArray.Length < DeweyData.Length)
                {
                    for( int i = 0; i < inputArray.Length; i++)
                    {
                        inputArray[i] = DeweyData[ri];
                        ri += 1;
                    }
                }
                else
                {
                    // starting point runs outside the data range
                    // so determine how much of an overrun and push the 
                    // starting point back within the data range
                    int overage = DeweyData.Length - ri;
                    int newri = ri - (inputArray.Length - overage);
                    for (int i = 0; i < inputArray.Length; i++)
                    {
                        inputArray[i] = DeweyData[newri];
                        newri += 1;
                    }
                }
            }


        }

        public void getWideListing(ref string[] inputArray)
        {
            // used by outside caller to pull wide ranged data
            if (inputArray == null || inputArray.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Array error encountered in getWideListing");
            }
            else
            {
                // use the bucket to store the randomally selected indexes
                // this prevents duplicate items
                List<int> indexBucket = new List<int>();
                int seed = Environment.TickCount;
                Random rnd = new Random(seed);
                int ri;
                int count = inputArray.Length;

                int i = 0;
                while (i < count)
                {
                    ri = rnd.Next(0, DeweyData.Length);
                    if (!indexBucket.Contains(ri))
                    {
                        indexBucket.Add(ri);
                        inputArray[i] = DeweyData[ri];
                        i++;
                    }
                }
            }
        }
    }
}
