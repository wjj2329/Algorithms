using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace _2_convex_hull
{
    class ConvexHullSolver
    {
        System.Drawing.Graphics g;
        System.Windows.Forms.PictureBox pictureBoxView;

        public ConvexHullSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBoxView)
        {
            this.g = g;
            this.pictureBoxView = pictureBoxView;
        }
        public void PrintList(List<PointF> mylist)
        {
            //Console.WriteLine("time to print the list");
            foreach (PointF x in mylist)
            {
                Console.WriteLine(x.X + " " + x.Y);
            }
        }

        public void Refresh()
        {
            // Use this especially for debugging and whenever you want to see what you have drawn so far
            pictureBoxView.Refresh();
        }

        public void Pause(int milliseconds)
        {
            // Use this especially for debugging and to animate your algorithm slowly
            pictureBoxView.Refresh();
            System.Threading.Thread.Sleep(milliseconds);
        }
    
        public PointF leftmost(List<PointF> list)
        {
            PointF leftmost = new PointF(1000000, 1000000);
            foreach (PointF point in list)
            {
                if (point.X < leftmost.X)
                {
                    leftmost = point;
                }
            }
            return leftmost;
        }
        public PointF rightmost(List<PointF> list)
        {
            PointF rightmost = new PointF(-1, -1);
            foreach (PointF point in list)
            {

                if (point.X > rightmost.X)
                {
                    rightmost = point;
                }
            }
            return rightmost;
        }
        public int getmelocationinlist(List<PointF> list1, PointF point)
        {
            int index = 0;
            foreach (PointF p in list1)
            {

                if (p == point)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        public double slope(PointF point1, PointF point2)
        {
            return (point2.Y - point1.Y) / (point2.X - point1.X);
        }
        public Tangent calculateuppertan(List<PointF> list1, List<PointF> list2)
        {
            Tangent tangent = new Tangent();
            PointF leftmostlist2 = leftmost(list2);//get leftmost from right hull
            PointF rightmostlist1 = rightmost(list1);//get rightmost from left hull
                                                     // Console.WriteLine("my right most for left hull is " + rightmostlist1);
                                                     //  Console.WriteLine("my left most for right hull is " + leftmostlist2);
            int leftmost2location = getmelocationinlist(list2, leftmostlist2);
            // Console.WriteLine("my left most location for the right one is " + leftmost2location);
            if (leftmost2location < list2.Count - 1)
            {
                leftmost2location++;
            }
            else
            {
                leftmost2location = 0;
            }
            while (slope(rightmostlist1, leftmostlist2) < slope(rightmostlist1, list2[leftmost2location]))
            {

                leftmostlist2 = list2[leftmost2location];
                leftmost2location++;
                if (leftmost2location > list2.Count - 1)
                {
                    leftmost2location = 0;
                }
            }
            //Console.WriteLine("the new leftmost is"+leftmostlist2);     THE UPPer right should be accurate through extensive testing aka right hulls upper left
            tangent.setRight(leftmostlist2);

            int rightmost1location = getmelocationinlist(list1, rightmostlist1);
            //Console.WriteLine("my right most for list 1 is " + rightmostlist1+" location is "+rightmost1location);
            if (rightmost1location != 0)
            {
                rightmost1location--;
            }
            else
            {
                rightmost1location = list1.Count - 1;
            }
            while (slope(rightmostlist1, leftmostlist2) > slope(list1[rightmost1location], leftmostlist2))
            {
                // Console.WriteLine(slope(rightmostlist1, leftmostlist2));
                //Console.WriteLine(slope(list1[rightmost1location],leftmostlist2 ));
                //Console.WriteLine("i set the upper tangent upper left to be " + list1[rightmost1location]);
                rightmostlist1 = list1[rightmost1location];
                rightmost1location--;
                if (rightmost1location < 0)
                {
                    rightmost1location = list1.Count - 1;
                }
            }
            tangent.setLeft(rightmostlist1);//I actually think this is correct lol
            return tangent;
        }
        public Tangent calculatelowertan(List<PointF> list1, List<PointF> list2) //k and m points on each side
        {
            Tangent tangent = new Tangent();
            PointF leftmostlist2 = leftmost(list2);
            PointF rightmostlist1 = rightmost(list1);
            int leftmost2location = getmelocationinlist(list2, leftmostlist2);
            // Console.WriteLine("my leftmost is this from list 2" + leftmostlist2);
            //Console.WriteLine("my right most is this from list 1" + rightmostlist1);

            if (leftmost2location != 0)
            {
                leftmost2location--;
            }
            else
            {
                leftmost2location = list2.Count - 1;
            }
            while (slope(rightmostlist1, leftmostlist2) > slope(rightmostlist1, list2[leftmost2location]))//slope should be negative
            {
                leftmostlist2 = list2[leftmost2location];
                leftmost2location--;
                if (leftmost2location < 0)
                {
                    leftmost2location = list2.Count - 1;
                }
            }
            // Console.WriteLine("i set the bottom right to be " + leftmostlist2);//this should be good
            tangent.setRight(leftmostlist2);

            int rightmost1location = getmelocationinlist(list1, rightmostlist1);
            if (rightmost1location < list1.Count - 1)
            {
                rightmost1location++;
            }
            else
            {
                rightmost1location = 0;
            }
            while (slope(rightmostlist1, leftmostlist2) < slope(list1[rightmost1location], leftmostlist2))//here!
            {
                //Console.WriteLine("the orginal slope is "+slope(rightmostlist1, leftmostlist2));
                //Console.WriteLine("the bigger is " + slope(list1[rightmost1location], leftmostlist2));
                rightmostlist1 = list1[rightmost1location];
                rightmost1location++;
                if (rightmost1location == list1.Count)
                {
                    rightmost1location = 0;
                }

            }
            //Console.WriteLine("i set the bototm left for list 1 " + rightmostlist1);
            tangent.setLeft(rightmostlist1);
            return tangent;

        }


        public List<System.Drawing.PointF> Merge(List<PointF> list1, List<PointF> list2)
        {
            Tangent uppertangent = new Tangent();
            Tangent lowertangent = new Tangent();
            List<PointF> listtoreturn = new List<PointF>();
            uppertangent = calculateuppertan(list1, list2);
            lowertangent = calculatelowertan(list1, list2);
            PointF start = leftmost(list1);
            int startposition = getmelocationinlist(list1, start);
            if (start == uppertangent.getLeft())
            {
                listtoreturn.Add(start);
            }
            else
            {
                while (start != uppertangent.getLeft())
                {
                    listtoreturn.Add(start);
                    startposition++;
                    if (list1.Count == startposition)
                    {
                        startposition = 0;
                    }
                    start = list1[startposition];
                }
                listtoreturn.Add(start);
            }
            start = uppertangent.getRight();
            startposition = getmelocationinlist(list2, start);
            if (start == lowertangent.getRight())
            {
                listtoreturn.Add(start);
            }
            else
            {
                while (start != lowertangent.getRight())
                {
                    listtoreturn.Add(start);
                    startposition++;
                    if (list2.Count == startposition)
                    {
                        startposition = 0;
                    }
                    start = list2[startposition];
                }
                listtoreturn.Add(start);
            }
            start = lowertangent.getLeft();
            startposition = getmelocationinlist(list1, start);
            if (start == leftmost(list1))
            {
            }
            else
            {
                while (start != leftmost(list1))
                {
                    listtoreturn.Add(start);
                    startposition++;
                    if (list1.Count == startposition)
                    {
                        startposition = 0;
                    }
                    start = list1[startposition];
                }
            }
            return listtoreturn;
        }
        public List<System.Drawing.PointF> DivideandCon(List<System.Drawing.PointF> pointList)  // A is 2 and B is 2   D is 1
        {
            //Console.Write("the size is " + pointList.Count);
            if (pointList.Count <= 3)
            {
                return pointList;
            }
            List<PointF> list1 = new List<PointF>();
            List<PointF> list2 = new List<PointF>();
            int half = pointList.Count / 2;
            for (int i = 0; i < pointList.Count; i++)
            {
                if (i >= half)
                {
                    list1.Add(pointList[i]);
                }
                else
                {
                    list2.Add(pointList[i]);
                }
            }
            List<PointF> stuff = Merge(DivideandCon(list2), DivideandCon(list1));
          
            return stuff;
        }
        public void Solve(List<System.Drawing.PointF> pointList)
        {
            pointList.Sort(delegate (PointF c1, PointF c2) { return c1.X.CompareTo(c2.X); });//this sorts it  n log n
            pointList = DivideandCon(pointList);
            if (pointList.Count == 1)//leave if only one point
            {
                return;
            }
            int i = 0;
            Pen mypen = new Pen(Color.Blue);
            for (i = 0; i < pointList.Count; i++)
            {

                if (i == pointList.Count - 1 && i != 0)
                {
                    g.DrawLine(mypen, pointList[i - 1], pointList[i]);
                    //Pause(100);
                }
                else
                {
                    g.DrawLine(mypen, pointList[i], pointList[i + 1]);
                   // Pause(1000);
                }
                //Console.WriteLine(pointList[i]);
            }
            g.DrawLine(mypen, pointList[i-1], pointList[0]);
           // stopwatch.Stop();
           // Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }
    }
}
class Tangent
{
    public Tangent()
    {

    }
    public int topleft;
    public int topright;
    public  int bottomleft;
    public int bottomright;

   
    PointF left;
    PointF right;
    public void setLeft(PointF left)
    {
        this.left = left;
    }
    public void setRight(PointF right)
    {
        this.right = right;
    }
    public PointF getLeft()
    {
        return left;
    }
    public PointF getRight()
    {
        return right;
    }


}