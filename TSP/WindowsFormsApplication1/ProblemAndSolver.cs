using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;


namespace TSP
{

    //Do weird stuff where you make matrix for all of them.  Then you make it small and get a 0 in every row and col and then add up all you 
    //took out.   So you get the lowest one each time and you move on from there.  Add the one you delete to the lower bound Keep ypdating matrix of evil and make it have one zero

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);

                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

    
        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        /// 
        public class Subproblem // a simple class to keep track of problems to solve for this project. 
        {
            public double score;
            public  double[,] mygraph;
            public List<int> mylist;
            public int location;
            public Subproblem(double score, double[,] mygraph, List<int>mylist, int location)
            {
                this.score = score;
                this.mygraph = mygraph;
                this.mylist = mylist;
                this.location = location;
                
            }
        }
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }
        
        public double[,] updatethegraph(double[,] mygraph, int x, int y)//creates a nice new graph pointer for my new sub-problem.  is N^2+n for time and n^2 for space for new array
        {

            double[,] temp = new double[Cities.Length, Cities.Length];
            for(int i=0; i<Cities.Length; i++)
            {
                for (int j=0; j<Cities.Length; j++)
                {
                    temp[i, j] = mygraph[i, j];
                }
            }
            for (int i=0; i<Cities.Length; i++)
            {
                temp[i, y] = Double.PositiveInfinity;
                temp[x, i] = Double.PositiveInfinity;
            }
            temp[y, x] = Double.PositiveInfinity;
            return temp;
        }
        
        void addsubproblems(double [,] oldgraph, int row,  int col, ref HashSet<Subproblem> priorityqueue,Subproblem currentnodetoadd, int location, double addcosttogethere, double bestscore, ref int numberofstatescreated , ref int numberofstatespruned)//adds thing to the queue
        {
            if(currentnodetoadd.score>bestscore)//pruning measure
            {
                numberofstatespruned++;
                return;
            }
            if (currentnodetoadd.mylist.Contains(location))//don't go inloops forever
            {
                return;
            }
            if(addcosttogethere==Double.PositiveInfinity)//another pruning measure
            {
                numberofstatespruned++;
                return;
            }
            double[,]newgraph = updatethegraph(oldgraph, row, col);//create new seprate graph an n^2 +n operation for time and n for space
            List<int> mylist = new List<int>();
            mylist.AddRange(currentnodetoadd.mylist);
            Subproblem temp = new Subproblem(currentnodetoadd.score, newgraph, mylist, location);          
            double lowerbound =normalizearray(ref temp.mygraph);//update graph an n^2+n operation and get lowerbound
            temp.score += lowerbound;
            temp.score += addcosttogethere;
            temp.mylist.Add(location);
            numberofstatescreated++;
            priorityqueue.Add(temp);

        }

        double normalizearray(ref double[,] mygraph)//an n^2+n opration to add infinities and get the lowerbound for the array should have combined it to be more efficient. 
        {
            double returning = 0;
            for (int x = 0; x < Cities.Length; x++)
            {
                bool haszero = false;
                double smallest=Double.PositiveInfinity;
                for (int y = 0; y < Cities.Length; y++)
                {
                   
                    if(smallest>mygraph[x,y])
                    {
                        smallest = mygraph[x, y];
                    }
                    if(mygraph[x,y]==0)
                    {
                        haszero = true;
                        break;
                    }
                }
                if(!haszero&&smallest!=Double.PositiveInfinity)
                {
                    for(int y=0; y<Cities.Length;y++)
                    {
                        mygraph[x, y] -= smallest;
                    }
                    returning += smallest;
                }
                
            }
    
            for (int x = 0; x < Cities.Length; x++)
            {
                bool haszero = false;
                double smallest = Double.PositiveInfinity;
                for (int y = 0; y < Cities.Length; y++)
                {
                    if (smallest > mygraph[y, x])
                    {
                        smallest = mygraph[y, x];
                    }
                    if (mygraph[y, x] == 0)
                    {
                        haszero = true;
                        break;
                    }
                }
                if (!haszero&&smallest!=Double.PositiveInfinity)
                {
                    for (int y = 0; y < Cities.Length; y++)
                    {
                        mygraph[y, x] -= smallest;
                    }
                    returning += smallest;
                }
            }
            return returning;
        }
      
  

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            int numberofupdates = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            HashSet<Subproblem> priorityqueue = new HashSet<Subproblem>(); //my pq is a simple list
            string[] results = new string[3];
            double[,] mygraph = new double[Cities.Length, Cities.Length];  //graph is a simple 2d array
            for (int x = 0; x < Cities.Length; x++)  //this sets up the intial matrix which is insignificant in the grad scheme of things. it would be n^2 for the number of cities but  this is only done once so its constant
            {
                for (int y = 0; y < Cities.Length; y++)
                {
                    if (x == y)
                    {
                        mygraph[x, y] = Double.PositiveInfinity;
                    }
                    else
                    {
                        mygraph[x, y] = Cities[x].costToGetTo(Cities[y]);
                    }
                }
            }
            double lowerbound= normalizearray(ref mygraph); //call my normalize function which is big o(n^2)
            List<int> path = new List<int>();    
            path.Add(0);
            Subproblem currentsubproblem = new Subproblem(lowerbound, mygraph, path, 0);
            priorityqueue.Add(currentsubproblem);
            double bestscore = Convert.ToDouble( greedySolveProblem()[COST]);  //call the greedy function which is n^2 to get a nice lower cost hopefully. 
            int numberofstatescreated = 0;
            int numberofstatespruned = 0;
            int maxiumumpqsize = 0;
            while (priorityqueue.Count>0) //While we can keep checking things
            {
                if(timer.Elapsed.TotalSeconds>TIME_LIMIT)
                {
                    break;
                }
                for (int i = 1; i < Cities.Length; i++) //n for the number of cities for time and also n subproblems each with n^2 matrices for space
                {                 
                    addsubproblems(currentsubproblem.mygraph, currentsubproblem.location, i, ref priorityqueue, currentsubproblem, i, currentsubproblem.mygraph[currentsubproblem.location,i], bestscore, ref numberofstatescreated, ref numberofstatespruned);//adds problems to explore see the function for description and analysis
                }
                if(priorityqueue.Count>maxiumumpqsize)
                {
                    maxiumumpqsize = priorityqueue.Count;
                }
                priorityqueue.Remove(currentsubproblem);//get rid of element we don't need
                if(priorityqueue.Count==0)
                {
                    break;//escape if we get to 0 stuff 
                }
                Subproblem bestone = new Subproblem(Double.MaxValue,null, path, -1);
                foreach (Subproblem s in priorityqueue) //n operation to get best element off of prioirty queue NOT the most efficient but still works. 
                {                  
                    if (s.score/(s.mylist.Count*2) < bestone.score/(bestone.mylist.Count*2))
                    {
                        bestone =s;
                    }
                }
                currentsubproblem = bestone;
                if(currentsubproblem.mylist.Count==Cities.Length)//if we have a potential solution
                {
                    Console.WriteLine("a solution approaches");
                    for(int i=0; i<currentsubproblem.mylist.Count; i++)
                    {
                        Console.WriteLine(currentsubproblem.mylist[i]);
                    }
                    if (currentsubproblem.score<bestscore &&Cities[currentsubproblem.mylist[currentsubproblem.mylist.Count-1]].costToGetTo(Cities[currentsubproblem.mylist[0]])!=Double.PositiveInfinity) //a check to see if we have a better score
                    {
                        numberofupdates++;
                        bestscore = currentsubproblem.score;
                        path = currentsubproblem.mylist;
                    }
                    if (Cities[currentsubproblem.mylist[currentsubproblem.mylist.Count - 1]].costToGetTo(Cities[currentsubproblem.mylist[0]]) == Double.PositiveInfinity)//bad if an infinity sneaks through
                    {
                        numberofstatespruned++;
                    }
                }
            }
            Route.Clear();
            if (path.Count != Cities.Length)
            {
                Console.WriteLine("failure");  //aka I wasn't able to get anything better then the GREEDY APROACH. 
            }
            else
            {
                //converts my path route into its equivalent City Route.   Just a constant time based on the number of Cities n SO pretty insigninficant. 
                for (int i = 0; i < path.Count; i++)
                {
                    Console.WriteLine(path[i]);
                    Route.Add(Cities[path[i]]);
                }
                bssf = new TSPSolution(Route);
            }
            timer.Stop();
            //print out my results for the table in the write up
            Console.WriteLine("My maximum queue size is " + maxiumumpqsize);
            Console.WriteLine("My maximum number of states created " + numberofstatescreated);
            Console.WriteLine("My maximum number of prunings I do is " + numberofstatespruned);
            results[COST] = Convert.ToString(bestscore);    
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = Convert.ToString(numberofupdates);

            return results;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            //this function is pretty simple really just an n^2 complexity due to the two for loops and n for space for the path I need. 
            Stopwatch timer = new Stopwatch();
            timer.Start(); 
            HashSet<City> alreadyvisted = new HashSet<City>();
            string[] results = new string[3];
            City currentcity = Cities[0];
            City BestCity = Cities[1];
            Route.Clear();
            for (int i = 0; i < Cities.Length; i++)
            {
                int number = -1;   
                for(int j=0; j<Cities.Length; j++)
                {
                    if (!Route.Contains(Cities[j]))
                    {
                        if (currentcity.costToGetTo(Cities[j]) < currentcity.costToGetTo(BestCity))
                        {
                            BestCity = Cities[j];
                            number = j;
                        }
                    }
                }
                currentcity = BestCity;
                Route.Add(currentcity);          
                BestCity = new City(100000000, 10000000);
            }
         
            bssf = new TSPSolution(Route);
            timer.Stop();
            results[COST] = Convert.ToString(bssf.costOfRoute());    // load results into array here, replacing these dummy values
            results[TIME] = Convert.ToString(timer.Elapsed.TotalSeconds);
            results[COUNT] = "1";

            return results;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
