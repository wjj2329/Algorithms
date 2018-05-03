using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneticsLab
{
    enum Direction {Up, Left, Diagonal, Finish };
   
    class PairWiseAlign
    {
        int MaxCharactersToAlign;

        public PairWiseAlign()
        {
            // Default is to align only 5000 characters in each sequence.
            this.MaxCharactersToAlign = 5000;
        }

        public PairWiseAlign(int len)
        {
            // Alternatively, we can use an different length; typically used with the banded option checked.
            this.MaxCharactersToAlign = len;
        }

        /// <summary>
        /// this is the function you implement.
        /// </summary>
        /// <param name="sequenceA">the first sequence</param>
        /// <param name="sequenceB">the second sequence, may have length not equal to the length of the first seq.</param>
        /// <param name="banded">true if alignment should be band limited.</param>
        /// <returns>the alignment score and the alignment (in a Result object) for sequenceA and sequenceB.  The calling function places the result in the dispay appropriately.
        /// 
        public void printmygraph(int[,] myarray, String word1, String word2)
        {
            for (int x = 0; x < word1.Length; x++)
            {

                for (int y = 0; y < word2.Length; y++)
                {
                    Console.Write(myarray[x, y]+" ");
                }
                Console.WriteLine();
            }
        }
        public ResultTable.Result Align_And_Extract(GeneSequence sequenceA, GeneSequence sequenceB, bool banded)
        {
            ResultTable.Result result = new ResultTable.Result();
            
            int score;        
            string[] alignment = new string[2];
            String word1;
            String word2;// place your two computed alignments here
            if (MaxCharactersToAlign < sequenceA.Sequence.Length)  //grabs the two words I need to compare and crops them to the right size if needed. 
            {
               word1 = sequenceA.Sequence.Substring(0, MaxCharactersToAlign);
            }
            else
            {
                word1 = sequenceA.Sequence;
            }
            if (MaxCharactersToAlign < sequenceB.Sequence.Length)
            {
               word2 = sequenceB.Sequence.Substring(0, MaxCharactersToAlign);
            }
            else
            {
                 word2 = sequenceB.Sequence;
            }
            
            word1=word1.Insert(0, "-");

            word2= word2.Insert(0, "-");//add dash to the front of each one like on the hw. 
            int[,] myarray = new int [word1.Length, word2.Length];  //array for costs
            Direction[,] mydirec = new Direction[word1.Length, word2.Length];  //array for back edges. 
            myarray[0, 0] = 0;//intialize first position for both of the arrays
            mydirec[0, 0] = Direction.Finish;
           

            for (int x=0; x<word1.Length; x++)
            {


                if (!banded)
                {
                    for (int y = 0; y < word2.Length; y++)  //go through both words  n by m is nm with n and m  being the length of the words. 
                    {
                        int left = int.MaxValue;
                        int up = int.MaxValue;
                        int diagonal = int.MaxValue;
                        if (x == 0 && y == 0)
                        {
                            continue;//don't do first time. 
                        }
                        if (y != 0) //check left
                        {
                            left = myarray[x, y - 1] + 5;
                        }
                        if (x != 0)//check up
                        {
                            up = myarray[x - 1, y] + 5;
                        }
                        if (x != 0 && y != 0)// check diagonal
                        {
                            if (word1[x] == word2[y])
                            {
                                diagonal = myarray[x - 1, y - 1] - 3;//if same -3
                            }
                            else
                            {
                                diagonal = myarray[x - 1, y - 1] + 1;//add 1 if not
                            }
                        }
                        int smallest = int.MaxValue;
                        Direction dir = Direction.Finish;
                        if (left < smallest) //get the best result
                        {
                            dir = Direction.Left;
                            smallest = left;
                        }
                        if (up <= smallest)
                        {
                            dir = Direction.Up;
                            smallest = up;
                        }
                        if (diagonal <= smallest)
                        {
                            dir = Direction.Diagonal;
                            smallest = diagonal;
                        }
                        myarray[x, y] = smallest;//set both arrays the results
                        mydirec[x, y] = dir;

                    }
                }
                
                else
                {
                    for (int y = x - 3; y < x + 4; y++)  //go through both words  n with k constant time being the banded length so nk in this case which is n.
                    {
                        if (y >= 0 && y < word2.Length)
                        {
                            int left = int.MaxValue;
                            int up = int.MaxValue;
                            int diagonal = int.MaxValue;
                            if (x == 0 && y == 0)
                            {
                                continue;//don't do first time. 
                            }
                            
                      
                            
                            if (y != 0)
                            {
                                left = myarray[x, y - 1] + 5;//check for left
                            }
                            if (x != 0)
                            {
                                up = myarray[x - 1, y] + 5;//check for up
                            }
                            if (x != 0 && y != 0) //check diagonal
                            {
                                if (word1[x] == word2[y])
                                {
                                    diagonal = myarray[x - 1, y - 1] - 3;
                                }
                                else
                                {
                                    diagonal = myarray[x - 1, y - 1] + 1;
                                }
                            }
                            int smallest = int.MaxValue;
                            Direction dir = Direction.Finish;//get smallest one. 
                            if (left < smallest)
                            {
                                dir = Direction.Left;
                                smallest = left;
                            }
                            if (up <= smallest)
                            {
                                dir = Direction.Up;
                                smallest = up;
                            }
                            if (diagonal <= smallest)
                            {
                                dir = Direction.Diagonal;
                                smallest = diagonal;
                            }
                            myarray[x, y] = smallest;//update both values
                            mydirec[x, y] = dir;

                        }
                    }
                }
                
            }
            score = myarray[word1.Length-1, word2.Length-1];  //set the score to the last element
            Direction begining = mydirec[word1.Length - 1, word2.Length - 1];
            alignment[0] = "";
            alignment[1] = "";
            int i = word1.Length - 1;
            int j = word2.Length - 1;
            if (score==0)//so if we cant' so it for banded stop now!
            {
                if (word2.Length > word1.Length+3)
                {
                    score = int.MaxValue;
                    alignment[0] = "No Alignment Possible";
                    alignment[1] = "No Alignment Possible";
                    result.Update(score, alignment[0], alignment[1]);          
                    return (result);
                }
            }
            StringBuilder alignment0=new StringBuilder(alignment[0]);
            StringBuilder alignment1 = new StringBuilder(alignment[1]);
            if (score == -6820)
            {
                Console.WriteLine(word1.Length);
                Console.WriteLine(word1[word1.Length-1]);
                Console.WriteLine(word2.Length);
                Console.WriteLine(word2[word2.Length-1]);
            }
            while(begining!=Direction.Finish)//iterate through the path to build the word  which is order m +n
            {
                if (score==-6820)
                {
                  //  Console.WriteLine(begining);
                    //Console.WriteLine(alignment0.ToString());
                    //Console.WriteLine(alignment1.ToString());
                    
                }
                switch (begining)
                {
                    case Direction.Left:
                        alignment0 = alignment0.Insert(alignment0.Length, Char.ToString('-'));
                        alignment1 = alignment1.Insert(alignment1.Length, Char.ToString(word2[j]));
                        j--;

                        break;
                    case Direction.Up:
                        alignment0 = alignment0.Insert(alignment0.Length, Char.ToString(word1[i]));
                        alignment1 = alignment1.Insert(alignment1.Length, Char.ToString('-'));          
                        i--;
                        break;
                    case Direction.Diagonal:
                        alignment0 = alignment0.Insert(alignment0.Length, Char.ToString(word1[i]));
                        alignment1 = alignment1.Insert(alignment1.Length, Char.ToString(word2[j]));
                        i--;
                        j--;
                        break;
                }
                begining = mydirec[i, j];
            }

            alignment[0] = alignment0.ToString();
            alignment[1] = alignment1.ToString();
            // ***************************************************************************************
            alignment[0] = new string(alignment[0].ToCharArray().Reverse().ToArray());//this would be another linear time to reverse it but still doesn't matter
            alignment[1] = new string(alignment[1].ToCharArray().Reverse().ToArray());
            if (alignment[0].Length > 100)
            {
                alignment[0] = alignment[0].Remove(100);
            }
            if (alignment[1].Length > 100)
            {
                alignment[1] = alignment[1].Remove(100);
            }

            result.Update(score,alignment[0],alignment[1]);                  // bundling your results into the right object type 
            return(result);
        }
    }
}
