using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apriori
{
    class Program
    {
        static void Main(string[] args)
        {
            // Lab Example Input
            float min_sup1 = 0.6f;
            float min_conf1 = 0.8f;
            Dictionary<int, string> data1 = new Dictionary<int, string>();
            data1.Add(100, "MONKEY");
            data1.Add(200, "DONKEY");
            data1.Add(300, "MAKE");
            data1.Add(400, "MUCKY");
            data1.Add(500, "COOKIE");

            // Lecture Example Input
            float min_sup2 = 0.3f;
            float min_conf2 = 0.7f;
            Dictionary<int, string> data2 = new Dictionary<int, string>();
            data2.Add(1, "ABCDEF");
            data2.Add(2, "ACDE");
            data2.Add(3, "CDE");
            data2.Add(4, "BF");
            data2.Add(5, "ABCD");
            data2.Add(6, "ABE");
            data2.Add(7, "ABC");
            data2.Add(8, "ABE");
            data2.Add(9, "AC");
            data2.Add(10, "ACDE");

            // Change to min_sup2, min_conf2, data2 to run Apriori on Lecture's example
            Apriori(min_sup1, min_conf1, data1);
        }

        static void Apriori(float minSup, float minConf, Dictionary<int, string> data)
        {
            int level = 1;
            Dictionary<string, int> candidates = new Dictionary<string, int>();
            List<Tuple<string, int>> frequentItemset = new List<Tuple<string, int>>();
            List<Tuple<string, string, float>> strongRules = new List<Tuple<string, string, float>>();
            Dictionary<int, List<Tuple<string, int>>> levelBasedFrequentItemset = new Dictionary<int, List<Tuple<string, int>>>();

            // Turning min_sup into count
            int min_sup = (int)(minSup * data.Count);

            // C1
            foreach (var transaction in data)
            {
                string itemset = transaction.Value;
                // Removing Duplicates
                string uniqueItemset = string.Join("", itemset.ToCharArray().Distinct());
                foreach (char item in uniqueItemset)
                {
                    bool exists = false;
                    foreach (var key in candidates.Keys)
                    {
                        if (item == key[0])
                        {
                            candidates[key]++;
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        candidates.Add(item.ToString(), 1);
                    }
                }
            }

            // C1 Output
            Console.WriteLine("C1 =");
            foreach (var item in candidates)
            {
                Console.WriteLine(item.Key + " : " + item.Value);
            }
            Console.WriteLine();

            // L1
            Dictionary<string, int> temp = new Dictionary<string, int>(candidates);
            candidates.Clear();
            foreach (var candidate in temp)
            {
                if (candidate.Value >= min_sup)
                {
                    candidates.Add(candidate.Key, candidate.Value);
                    frequentItemset.Add(new Tuple<string, int>(candidate.Key,candidate.Value));
                }
            }
            levelBasedFrequentItemset.Add(1, new List<Tuple<string,int>>(frequentItemset));
            frequentItemset.Clear();

            // L1 Output
            Console.WriteLine("L1 =");
            foreach (var item in candidates)
            {
                Console.WriteLine(item.Key + " : " + item.Value);
            }
            Console.WriteLine();

            level++;
            while (candidates.Count > 0)
            {
                // C2 and above
                List<string> candidatesList = candidates.Keys.ToList();
                candidates.Clear();
                for (int i = 0; i < candidatesList.Count; i++)
                {
                    for (int j = i + 1; j < candidatesList.Count; j++)
                    {
                        // Taking first k-1 common 
                        bool rule = true;
                        for (int k = 0; k < level - 2; k++)
                        {
                            if (candidatesList[i][k] != candidatesList[j][k])
                            {
                                rule = false;
                            }
                        }
                        if (rule)
                        {
                            string newCandidate = "";
                            // Merging 2 candidates together
                            newCandidate = candidatesList[i] + candidatesList[j][level-2];

                            // Pruning canditates if possible
                            bool isPruned = false;
                            for (int x = 0; x < level - 2; x++)
                            {
                                int subArraySize = (level - 1) - x;
                                int requiredSubArrays = Combination(level,subArraySize);
                                int counter = 0;
                                foreach (var itemset in levelBasedFrequentItemset[subArraySize])
                                {
                                    bool exists = true;
                                    foreach(char item in itemset.Item1)
                                    {
                                        if(!newCandidate.Contains(item))
                                        {
                                            exists = false;
                                            break;
                                        }
                                    }
                                    if(exists)
                                    {
                                        counter++;
                                    }
                                }
                                if(counter < requiredSubArrays)
                                {
                                    isPruned = true;
                                }
                            }

                            // Checking for frequency of new canditate items together
                            if (!isPruned)
                            {
                                int freq = 0;
                                foreach (var transaction in data)
                                {
                                    int counter = 0;
                                    foreach (char item in newCandidate)
                                    {
                                        if (transaction.Value.Contains(item))
                                        {
                                            counter++;
                                        }
                                    }
                                    if (counter == newCandidate.Length)
                                    {
                                        freq++;
                                    }
                                }
                                candidates.Add(newCandidate, freq);
                            }
                            else
                            {
                                Console.WriteLine(newCandidate + " is pruned");
                            }
                        }
                    }
                }
                if (candidates.Count == 0)
                    break;

                // C2 and above Output
                Console.WriteLine("C" + level + " =");
                foreach (var item in candidates)
                {
                    Console.WriteLine(item.Key + " : " + item.Value);
                }
                Console.WriteLine();

                // L2 and above
                temp = new Dictionary<string, int>(candidates);
                candidates.Clear();
                foreach (var candidate in temp)
                {
                    if (candidate.Value >= min_sup)
                    {
                        candidates.Add(candidate.Key, candidate.Value);
                        frequentItemset.Add(new Tuple<string, int>(candidate.Key, candidate.Value));
                    }
                }
                levelBasedFrequentItemset.Add(level, new List<Tuple<string, int>>(frequentItemset));
                frequentItemset.Clear();

                // L2 and above Output
                Console.WriteLine("L" + level + " =");
                foreach (var item in candidates)
                {
                    Console.WriteLine(item.Key + " : " + item.Value);
                }
                Console.WriteLine();

                level++;
            }

            // Finding Strong Rules
            Console.WriteLine("All Rules for frequent itemsets:");
            for(int i=2; i<=levelBasedFrequentItemset.Count; i++)
            {
                foreach(var itemset in levelBasedFrequentItemset[i])
                {
                    // 0 : not strong  1 : strong
                    Dictionary<string, int> leftSideRules = new Dictionary<string, int>();
                    for(int j=1; j<=Math.Pow(2,i)-2; j++)
                    {
                        string selection = DecimalToBinary(j, i);
                        string left = "";
                        string right = "";
                        for(int k=0; k<i; k++)
                        {
                            if(selection[k] == '0')
                            {
                                left += itemset.Item1[k];
                            }
                            else
                            {
                                right += itemset.Item1[k];
                            }
                        }

                        // Pruning rules if possible
                        bool isPruned = false;
                        if(left.Length < i - 1)
                        {
                            for(int k=0; k<right.Length; k++)
                            {
                                string tempLeft = left + right[k];
                                foreach(var rule in leftSideRules)
                                {
                                    if(HaveSameLetters(rule.Key,tempLeft) && rule.Value == 0)
                                    {
                                        isPruned = true;
                                        break;
                                    }
                                }
                                if(isPruned)
                                {
                                    break;
                                }
                            }
                        }

                        // Finding and Calculating Confidence
                        if (!isPruned)
                        {
                            int top = itemset.Item2;
                            int bot = 1;
                            foreach (var item in levelBasedFrequentItemset[left.Length])
                            {
                                if (item.Item1.Equals(left))
                                {
                                    bot = item.Item2;
                                    break;
                                }
                            }
                            float conf = (float)top / (float)bot;
                            if (conf >= minConf)
                            {
                                strongRules.Add(new Tuple<string, string, float>(left, right, conf));
                            }

                            // Rules Output
                            Console.Write(left + " -> " + right + " conf = " + top + "/" + bot + " = " + conf * 100 + "%");
                            if (conf >= minConf)
                            {
                                Console.WriteLine(" (Strong Rule)");
                                leftSideRules.Add(left,1);
                            }
                            else
                            {
                                Console.WriteLine(" (Not Strong Rule)");
                                leftSideRules.Add(left,0);
                            }
                        }

                        // If rule is pruned
                        else
                        {
                            Console.WriteLine(left + " -> " + right + " is pruned");
                        }
                    }
                    Console.WriteLine();
                }
            }

            // Frequent Itemsets Output
            Console.Write("Frequent Itemsets:\n{");
            foreach (var frequentSet in levelBasedFrequentItemset.Values)
            {
                foreach (var item in frequentSet)
                {
                    Console.Write("{" + item.Item1 + "}, ");
                }
            }
            Console.WriteLine("}\n");

            // Strong Rules Output
            Console.WriteLine("Strong Rules:");
            foreach(var rule in strongRules)
            {
                Console.WriteLine(rule.Item1 + " -> " + rule.Item2 + " (" + rule.Item3 * 100 + "% Confidence)");
            }
            Console.WriteLine();
        }

        // Function to Calculate Combination (nCr = n! / r! (n - r)!)
        static int Combination(int n, int r)
        {
            int res = 1;

            int factN = 1;
            for(int i=2; i<=n; i++)
            {
                factN *= i;
            }

            int factR = 1;
            for (int i = 2; i <= r; i++)
            {
                factR *= i;
            }

            int factNR = 1;
            for (int i = 2; i <= (n-r); i++)
            {
                factNR *= i;
            }

            res = factN / (factR * factNR);

            return res;
        }

        // Function to convert a decimal integer into a binary string
        static string DecimalToBinary(int n, int size)
        {
            string binary = "";
            while(n>0)
            {
                binary += n % 2;
                n /= 2;
            }

            int binarySize = binary.Length;
            for(int i=0; i<size-binarySize; i++)
            {
                binary += '0';
            }

            // Reversing the string
            char[] arr = binary.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        // Function to check if 2 strings have the same letters (order doesn't matter)
        static bool HaveSameLetters(string a, string b)
        {
            return a.All(x => b.Contains(x)) && b.All(x => a.Contains(x));
        }

    }
}
