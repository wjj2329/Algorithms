using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace NetworkRouting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void clearAll()
        {
            startNodeIndex = -1;
            stopNodeIndex = -1;
            sourceNodeBox.Clear();
            sourceNodeBox.Refresh();
            targetNodeBox.Clear();
            targetNodeBox.Refresh();
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            arrayCheckBox.Checked = false;
            arrayCheckBox.Refresh();
            return;
        }

        private void clearSome()
        {
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            return;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            int randomSeed = int.Parse(randomSeedBox.Text);
            int size = int.Parse(sizeBox.Text);

            Random rand = new Random(randomSeed);
            seedUsedLabel.Text = "Random Seed Used: " + randomSeed.ToString();

            clearAll();
            this.adjacencyList = generateAdjacencyList(size, rand);
            List<PointF> points = generatePoints(size, rand);
            resetImageToPoints(points);
            this.points = points;
        }

        // Generates the distance matrix.  Values of -1 indicate a missing edge.  Loopbacks are at a cost of 0.
        private const int MIN_WEIGHT = 1;
        private const int MAX_WEIGHT = 100;
        private const double PROBABILITY_OF_DELETION = 0.35;

        private const int NUMBER_OF_ADJACENT_POINTS = 3;

        private List<HashSet<int>> generateAdjacencyList(int size, Random rand)
        {
            List<HashSet<int>> adjacencyList = new List<HashSet<int>>();

            for (int i = 0; i < size; i++)
            {
                HashSet<int> adjacentPoints = new HashSet<int>();
                while (adjacentPoints.Count < 3)
                {
                    int point = rand.Next(size);
                    if (point != i) adjacentPoints.Add(point);
                }
                adjacencyList.Add(adjacentPoints);
            }

            return adjacencyList;
        }

        private List<PointF> generatePoints(int size, Random rand)
        {
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new PointF((float)(rand.NextDouble() * pictureBox.Width), (float)(rand.NextDouble() * pictureBox.Height)));
            }
            return points;
        }

        private void resetImageToPoints(List<PointF> points)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            Pen pen;

            if (points.Count < 100)
                pen = new Pen(Color.Blue);
            else
                pen = new Pen(Color.LightBlue);
            foreach (PointF point in points)
            {
                graphics.DrawEllipse(pen, point.X, point.Y, 2, 2);
            }

            this.graphics = graphics;
            pictureBox.Invalidate();
        }

        // These variables are instantiated after the "Generate" button is clicked
        private List<PointF> points = new List<PointF>();
        private Graphics graphics;
        private List<HashSet<int>> adjacencyList;

        // Use this to generate paths (from start) to every node; then, just return the path of interest from start node to end node
        private void solveButton_Click(object sender, EventArgs e)
        {
            // This was the old entry point, but now it is just some form interface handling
            bool ready = true;

            if (startNodeIndex == -1)
            {
                sourceNodeBox.Focus();
                sourceNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if (stopNodeIndex == -1)
            {
                if (!sourceNodeBox.Focused)
                    targetNodeBox.Focus();
                targetNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if (points.Count > 0)
            {
                resetImageToPoints(points);
                paintStartStopPoints();
            }
            else
            {
                ready = false;
            }
            if (ready)
            {
                clearSome();
                solveButton_Clicked();  // Here is the new entry point
            }
        }

        double calculatedistance(PointF point1, PointF point2)
        {
            return Math.Abs(( Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + (Math.Pow((point2.Y - point1.Y), 2)))));
        }
        
        

        private void solveButton_Clicked()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            double[] dist = new double[points.Count];
            double[] prev = new double[points.Count];

            for (int i=0; i<points.Count; i++)
            {
                dist[i] = double.MaxValue;
                prev[i] = -1;
            }
            //Console.Write("I GET HERE");
            dist[startNodeIndex] = 0;
            PriorityQueue myqueue =new ArrayQueue(dist);
           //Console.WriteLine("i make it here");
            while(myqueue.getSize()>0)
            {
               //Console.WriteLine("i check for removed");
                int removed = myqueue.remove();
                //Console.WriteLine("my removed is " + removed);
                
                foreach (int x in adjacencyList[removed])
                {
                    //Console.WriteLine(x);
                    double newdistance = dist[removed]+calculatedistance(points[removed], points[x]);
                    if(newdistance<dist[x])
                    {
                        dist[x] = newdistance;
                        prev[x] = removed;
                       // Console.WriteLine("my prev is "+removed);
                        myqueue.update(x, newdistance);
                    }
                }
               
            }
            //Console.WriteLine("I do get out of the main thing");
            List<double> reversepathstuff = new List<double>();
            double temp = stopNodeIndex;
          
          
            while(temp!=startNodeIndex)
            {
                    reversepathstuff.Add(temp);
                temp = prev[(int)temp];
            }
            reversepathstuff.Add(temp);
            //Console.WriteLine("I survive to here as well");
            double total = 0;
            for (int i = 0; i < reversepathstuff.Count; i++)
            {
                if (i != reversepathstuff.Count - 1)
                {
                    graphics.DrawLine(Pens.Blue, points[(int)reversepathstuff[i]], points[(int)reversepathstuff[i + 1]]);
                    graphics.DrawString(Convert.ToString((int)calculatedistance(points[(int)reversepathstuff[i]],points[(int)reversepathstuff[i+1]])), new Font("Ariel", 12), new SolidBrush(System.Drawing.Color.Black),new PointF((points[(int)reversepathstuff[i]].X+ points[(int)reversepathstuff[i + 1]].X)/2, (points[(int)reversepathstuff[i]].Y + points[(int)reversepathstuff[i + 1]].Y) / 2));
                    total += calculatedistance(points[(int)reversepathstuff[i]], points[(int)reversepathstuff[i + 1]]);
                }
            }
            pathCostBox.Text = Convert.ToString(total);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10.0);
            arrayTimeBox.Text = elapsedTime;

            pathCostBox.Text = Convert.ToString(total);
            Console.WriteLine(elapsedTime);
            Console.WriteLine(ts.TotalMilliseconds);
            //points connect to adjacency list sets corresponding on the index


            // *** Implement this method, use the variables "startNodeIndex" and "stopNodeIndex" as the indices for your start and stop points, respectively ***

        }
        
        private Boolean startStopToggle = true;
        private int startNodeIndex = -1;
        private int stopNodeIndex = -1;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (points.Count > 0)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                int index = ClosestPoint(points, mouseDownLocation);
                if (startStopToggle)
                {
                    startNodeIndex = index;
                    sourceNodeBox.ResetBackColor();
                    sourceNodeBox.Text = "" + index;
                }
                else
                {
                    stopNodeIndex = index;
                    targetNodeBox.ResetBackColor();
                    targetNodeBox.Text = "" + index;
                }
                resetImageToPoints(points);
                paintStartStopPoints();
            }
        }

        private void sourceNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try{ startNodeIndex = int.Parse(sourceNodeBox.Text); }
                catch { startNodeIndex = -1; }
                if (startNodeIndex < 0 | startNodeIndex > points.Count-1)
                    startNodeIndex = -1;
                if(startNodeIndex != -1)
                {
                    sourceNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }

        private void targetNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try { stopNodeIndex = int.Parse(targetNodeBox.Text); }
                catch { stopNodeIndex = -1; }
                if (stopNodeIndex < 0 | stopNodeIndex > points.Count-1)
                    stopNodeIndex = -1;
                if(stopNodeIndex != -1)
                {
                    targetNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }
        
        private void paintStartStopPoints()
        {
            if (startNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Green, 6), points[startNodeIndex].X, points[startNodeIndex].Y, 1, 1);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }

            if (stopNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Red, 2), points[stopNodeIndex].X - 3, points[stopNodeIndex].Y - 3, 8, 8);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }
        }

        private int ClosestPoint(List<PointF> points, Point mouseDownLocation)
        {
            double minDist = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double dist = Math.Sqrt(Math.Pow(points[i].X-mouseDownLocation.X,2) + Math.Pow(points[i].Y - mouseDownLocation.Y,2));
                if (dist < minDist)
                {
                    minIndex = i;
                    minDist = dist;
                }
            }

            return minIndex;
        }

    }
    
}

public class Node  //A simple node class only used in the heap
{
    public double distance;
    public int index;
    public int whereami;
    public Node(double distance, int index, int whereami)
    {
        this.distance = distance;
        this.index = index;
        this.whereami = whereami;
    }
}

public class Heap:PriorityQueue //A Heap implmentation of Priority Queue
{
    public Heap(double[] distances, int startindex)
    {
        for (int i=0; i<distances.Length; i++)
        {
            Node temp = new Node(distances[i], i, i);
            mylist.Add(temp);
            mymap.Add(i, temp);
        }
        Swap(mylist, 0, startindex);

    }
   
    public void Swaplocations(List<int>locations, int indexA, int indexB)
    {
        int tmp = locations[indexA];
        locations[indexA] = locations[indexB];
        locations[indexB] = tmp;
    }

    public void Swap(List<Node> list, int indexA, int indexB)
    {
        Node tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
    private Dictionary<int, Node> mymap = new Dictionary<int, Node>();

    //some nice debugging stuff not used
    void printwhattheythink()
    {
        for (int i = 0; i < mylist.Count; i++)
        {
            Console.WriteLine("for locations " + i);
            if (mymap.ContainsKey(i))
            {
                Console.WriteLine("he is located at " + mymap[i].whereami);
            }
            else
            {
                Console.WriteLine("he is located at "+-1);
            }
        }
    }
    private int findindexfornode(int index)    //some nice debugging stuff not used

    {
        for (int i = 0; i < mylist.Count; i++)
        {
            if (mylist[i].index == index)
            {

                return i;
            }
        }
        return -1;
    }
    void printindexfornodes()    //some nice debugging stuff not used

    {
        for ( int i=0; i<mylist.Count; i++)
        {
            Console.WriteLine("for locations " + i);
            Console.WriteLine("he is located at " + findindexfornode(i));
        }
                
    }
   
   
    List<Node> mylist = new List<Node>();
    void PriorityQueue.update(int indexupdated, double newdistance)
    {
        Node update = mymap[indexupdated];
        int currentindex = findindexfornode(indexupdated);
        update.distance = newdistance;
        int parentindex = ((currentindex - 1) / 2);
        if(parentindex<0)
        {
            return;
        }
        while(mylist[parentindex].distance>mylist[currentindex].distance)
        {
            if (parentindex < 0)
            {
                return;
            }
            Swap(mylist, parentindex, currentindex);
            currentindex = parentindex;
            parentindex= ((currentindex - 1) / 2);
        }
    }
    void trickledown(Node current, int currentindex)
    {
        int leftchild = 2 * currentindex + 1;
        if(leftchild>=mylist.Count)
        {
            return;
        }
        int rightchild = 2 * currentindex + 2;
        if(rightchild>=mylist.Count)
        {
            return;
        }
       // Console.WriteLine("i see left child with value " + leftchild);

        if (current.distance>mylist[leftchild].distance)
        {
            Swap(mylist, leftchild, currentindex);

            trickledown(mylist[leftchild], leftchild);  
        }
       // Console.WriteLine("i see right child with value " + rightchild);
        if(current.distance>mylist[rightchild].distance)
        {
            Swap(mylist, rightchild, currentindex);

            trickledown(mylist[rightchild], rightchild);
        }
    }
    int PriorityQueue.remove()
    {

        Node returning= mylist[0];
        int indextoreturn = returning.index;
        if (mylist.Count > 1)
        {
            mylist[0] = mylist[mylist.Count - 1];
            mylist.RemoveAt(mylist.Count - 1);
            trickledown(mylist[0], 0);

        }
        else
        {
            mylist.RemoveAt(0);
        }
        return indextoreturn;
    }
    int PriorityQueue.getSize()
    {
        return mylist.Count;
    }
}
public class ArrayQueue:PriorityQueue //Prioirty Queue implemenation. 
{
    List<double> mylist = new List<double>();
    int size;
    public ArrayQueue(double []distances)
    {
        for(int i=0; i<distances.Length; i++)
        {
            mylist.Add(distances[i]);
        }
        size = distances.Length;
    }
     void PriorityQueue.update(int indexupdated,  double newdistance)
    {
      //  Console.WriteLine("I UPDATE");
        mylist[indexupdated] = newdistance;
    }

     int PriorityQueue.remove()
    {
        int temp = -1;
        double biggest = double.PositiveInfinity;
       // Console.WriteLine(" I DO SURVIVE");

        for (int i=0; i<mylist.Count; i++)
        {
            if((mylist[i]<biggest)&&(mylist[i]!=-1))
            {
                biggest = mylist[i];
                temp = i;
            }

        }
        size--;
        mylist[temp] = -1;

        return temp;
    }
   
     int PriorityQueue.getSize()
    {
        return size;
    }

}

public interface PriorityQueue
{
    void update(int indexupdated, double newdistance);
    int remove();
    int getSize();
}