using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DeweySort_v1._0
{

    // DeweySort project and code provided as open source,
    // free of all restrictions or support.
    
    public partial class Form1 : Form
    {       
        private static DeweyDataManager DDM;
        private string[] sortedItems;
        private string[] shuffledItems;
        int elapsedSeconds;

        public Form1()
        {
            InitializeComponent();

            // change this location to your own file location
            string dataFile = @"C:\Dewey_Data\DeweyData.txt";
            DDM = new DeweyDataManager(dataFile);
            // load up all the data in the file
            DDM.initializeData();

            if (!DDM.dataIsAvailable())
            {
                // no file data available so show error message and
                // disable all controls.
                string msg = "";
                msg += "There is a problem loading the file data.\n\r";
                msg += DDM.errorMessage();
                MessageBox.Show(msg);
                disableControls();
            }

            // set up the drag and drop for the control
            listView1.MultiSelect = false; 
            listView1.AllowDrop = true;
            listView1.HideSelection = false;
            listView1.View = View.List;  
            listView1.DragDrop += new DragEventHandler(listView1_DragDrop);
            listView1.DragEnter += new DragEventHandler(listView1_DragEnter);            
            listView1.ItemDrag += new ItemDragEventHandler(listView1_ItemDrag);

            // 1 second count
            timer1.Interval = 1000;
            elapsedSeconds = 0;
            // user feedback
            updateStatus();
        }

        void listView1_DragDrop(object sender, DragEventArgs e)
        {
            // used during drag and drop
            ListViewItem item = e.Data.GetData(typeof(ListViewItem)) as
            ListViewItem;

            if (item != null)
            {
                Point pt = this.listView1.PointToClient(new Point(e.X, e.Y));
                ListViewItem hoveritem = this.listView1.GetItemAt(pt.X, pt.Y);
                if (hoveritem != null)
                {
                    if (item != hoveritem)  // can't drop on yourself
                    {
                        // roughly, drop above if over 1/2 height
                        //          drop below of under 1/2 height
                        Rectangle rc = hoveritem.GetBounds(ItemBoundsPortion.Entire);
                        bool insertBefore;
                        if (pt.Y < rc.Top + (rc.Height / 2))
                            insertBefore = true;
                        else
                            insertBefore = false;

                        listView1.Items.Remove(item);

                        if (insertBefore)
                        {
                            listView1.Items.Insert(hoveritem.Index, item);
                        }
                        else
                        {
                            listView1.Items.Insert(hoveritem.Index + 1, item);
                        }
                    }

                }
            }
        }

        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            // used during drag and drop
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // used during drag and drop
            listView1.DoDragDrop(this.listView1.SelectedItems[0],
            DragDropEffects.All);
        }

        private void cmdLoadRandom10_Click(object sender, EventArgs e)
        {
            // loads sort list
            loadDewyCallNumbers(10);
        }

        private void cmdLoadRandom50_Click(object sender, EventArgs e)
        {
            // loads sort list
            loadDewyCallNumbers(50);
        }

        private void cmdLoadRandom75_Click(object sender, EventArgs e)
        {
            // loads sort list
            loadDewyCallNumbers(75);
        }

        private void cmdLoadRandom100_Click(object sender, EventArgs e)
        {
            // loads sort list
            loadDewyCallNumbers(100);
        }

        private void loadDewyCallNumbers(int count)
        {
            //TODO someday: validate count 
            // this is where list gets loaded
            loadWorkingArrays(count); 
            for (int i = 0; i < shuffledItems.Length; i++)
            {
                listView1.Items.Add(shuffledItems[i]);
            }

            updateStatus();
        }

        private void loadWorkingArrays(int size)
        {
            //TODO someday: Error check size > 0
            // sorted and shuffled are used to check sort order
            sortedItems = null;
            shuffledItems = null;

            sortedItems = new string[size];

            if (optRangeWide.Checked)  
            {
                DDM.getWideListing(ref sortedItems);
            }
            else  // narrow range
            {
                DDM.getNarrowListing(ref sortedItems); 
            }
             
            sortArray(ref sortedItems);

            shuffledItems = new string[sortedItems.Length];
            Array.Copy(sortedItems, shuffledItems, sortedItems.Length);
            shuffleArray(ref shuffledItems); 

        }

        // used for testing
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        //    ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
        //    foreach (int index in indexes)
        //    {
        //        txtStatus.Text = listView1.Items[index].Text;
        //    }
        }

        private void updateStatus()
        {
            // give user feedback
            if (!DDM.dataIsAvailable())
            {
                lblStatus.Text = "ERROR : " + DDM.errorMessage();
            }
            else
            {
                //lblStatus.Text = "Items: " + listView1.Items.Count.ToString();
                lblItemCount.Text = "Items: " + listView1.Items.Count.ToString();
                lblFontSize.Text = "Size: " + listView1.Font.Size.ToString(); 
            }
        }

        private void cmdCheckResults_MouseDown(object sender, MouseEventArgs e)
        {
            // remove any item selections in the list
            foreach (ListViewItem i in listView1.SelectedItems)
            {
                i.Selected = false;
            }
        } 

        private void cmdCheckResults_Click(object sender, EventArgs e)
        {
            // feedback
            checkAndReport();
        }

        private void checkAndReport()
        {
            //TODO someday: error check for no items in list
            //TODO someday: error check for null arrays
            //TODO someday: error check for items in listView

            int count = listView1.Items.Count;
            string[] currentList = new string[count];
            string[] sortedList = new string[count];
            // get two copies of the data that has been sorted by the user
            foreach (ListViewItem item in listView1.Items)
            {
                currentList[item.Index] = item.Text;
                sortedList[item.Index] = item.Text;
            }

            // use software to sort one copy
            sortArray(ref sortedList);

            // determine how many don't match, i.e. how many sort errors
            int sortError = 0;
            int sortCorrect = 0;
            for (int i = 0; i < currentList.Length; i++)
            {
                if (sortedList[i] == currentList[i])
                {
                    sortCorrect++;
                }
                else
                {
                    sortError++;
                }
            }

            // feedback
            string resultMsg = "";
            resultMsg += "Total items: " + currentList.Length.ToString() + "\r\n";
            resultMsg += "Correct: " + sortCorrect.ToString() + "\r\n";
            resultMsg += "Errors: " + sortError.ToString() + "\r\n";

            // more feedback
            if (currentList.Length != 0)
            {
                float score = 0;
                score = ((float)sortCorrect / (float)currentList.Length) * 100;
                resultMsg += "Score: " + score.ToString("0.0") + " %";
            }

            // feedback
            txtStatus.Text = resultMsg;

        }

        private void cmdShowErrors_MouseDown(object sender, MouseEventArgs e)
        {
            checkAndReport();

            // first clear any left over selections
            foreach (ListViewItem i in listView1.SelectedItems)
            {
                i.Selected = false;
            }

            // get a current and a sorted version of th elisting
            int count = listView1.Items.Count;
            string[] currentList = new string[count];
            string[] sortedList = new string[count];
            int[] errorIndex = new int[count];
            foreach (ListViewItem item in listView1.Items)
            {
                currentList[item.Index] = item.Text;
                sortedList[item.Index] = item.Text;
            }

            sortArray(ref sortedList);

            // if there is a difference then that is a sort error
            // so identify that index
            int errorCount = 0;
            for (int i = 0; i < currentList.Length; i++)
            {
                if (sortedList[i] == currentList[i])
                {
                    errorIndex[i] = 0;  // 0 = no error
                }
                else
                {
                    errorIndex[i] = 1;  // 1 = error
                    errorCount++;
                }
            }

            //turn on multiselect and highlight all the errors
            listView1.MultiSelect = true;

            for (int i = 0; i < errorIndex.Length; i++)
            {
                if (errorIndex[i] == 1)  // error exists
                {
                    listView1.Items[i].Focused = true;
                    listView1.Items[i].Selected = true;
                }
            }

            // turn off multiselect 
            listView1.MultiSelect = false;

        }

        private void cmdShowErrors_MouseUp(object sender, MouseEventArgs e)
        {
            // when user lets go of the mouse, clear the highlights
            foreach (ListViewItem i in listView1.SelectedItems)
            {
                i.Selected = false;
            }
        }

        private void cmdClearList_Click(object sender, EventArgs e)
        {
            // remove current list of items to be sorted
            listView1.Clear();
            sortedItems = null;
            shuffledItems = null;
            txtStatus.Text = "";
            updateStatus();
        }

        private void cmdStartTiming_Click(object sender, EventArgs e)
        {
            // start the stopwatch
            timer1.Start();
            if (elapsedSeconds == 0)
                elapsedSeconds = 1;
        }

        private void cmdStopTiming_Click(object sender, EventArgs e)
        {
            // stop/pause the stopwatch 
            timer1.Stop();
        }

        private void cmdReset_Click(object sender, EventArgs e)
        {
            // reset the stopwatch
            elapsedSeconds = 0;
            lblTimer.Text = TimeSpan.FromSeconds(elapsedSeconds).ToString();
            lblTimer.Refresh();   
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // every second, update the stopwatch display
            if (chkShowTimer.Checked)
            {
                lblTimer.Text = TimeSpan.FromSeconds(elapsedSeconds).ToString();
                lblTimer.Refresh();
            }

            elapsedSeconds++;
        }

        private void chkShowTimer_CheckedChanged(object sender, EventArgs e)
        {
            // show or hide the stopwatch display
            if (!chkShowTimer.Checked)
            {
                lblTimer.Text = "";
            }
            else
            {
                lblTimer.Text = TimeSpan.FromSeconds(elapsedSeconds).ToString();
                lblTimer.Refresh();
            }
        }
        
        private void cmdFont_Click(object sender, EventArgs e)
        {
            // let the user change the font size and style
            // some non-standard fonts cause an error
            //TODO someday: error check the selected font
            DialogResult result = fontDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Font font = fontDialog1.Font;
                listView1.Font = font;
                listView1.Refresh();
            }

            // reload the list after font change inorder to 
            // prevent elipsis (...)
            int count = listView1.Items.Count;
            string[] reLoad = new string[count];

            foreach (ListViewItem item in listView1.Items)
            {
                reLoad[item.Index] = item.Text;
            }

            listView1.Clear();
            txtStatus.Text = "";

            for (int i = 0; i < reLoad.Length; i++)
            {
                listView1.Items.Add(reLoad[i]);
            }

            updateStatus();

        }

        private void cmdShuffleCurrentList_Click(object sender, EventArgs e)
        {
            // shuffle the current list without changing the items
            int count = listView1.Items.Count;
            string[] reShuffle = new string[count];

            foreach (ListViewItem item in listView1.Items)
            {
                reShuffle[item.Index] = item.Text;
            }

            shuffleArray(ref reShuffle);

            listView1.Clear();
            txtStatus.Text = "";

            for (int i = 0; i < reShuffle.Length; i++)
            {
                listView1.Items.Add(reShuffle[i]);
            }

        }

        private void shuffleArray(ref string[] inputArray)
        {
            ///TODO someday:  Error check, inputArray != null
            // used to provide a random order of items
            int second;
            string temp;
            int seed = (int)DateTime.Now.Ticks;
            Random rnd = new Random(seed);

            for (int first = 0; first < inputArray.Length; first++)
            {
                second = rnd.Next(inputArray.Length);
                temp = inputArray[first];
                inputArray[first] = inputArray[second];
                inputArray[second] = temp;
            }   

        }

        private void sortArray(ref string[] inputArray)
        {
            /// TODO someday:  Error check, inputArray != null

            // This is a seperate function in case there is a need for specialized sort code
            // because different data is used. 
            // for now, just use standard array sort
            Array.Sort(inputArray);
        }

        private void disableControls()
        {
            // disable controls if data file is not found
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
            cmdCheckResults.Enabled = false;
            cmdClearList.Enabled = false;
            cmdShuffleCurrentList.Enabled = false;
            cmdShowErrors.Enabled = false; 
        }

    }
}