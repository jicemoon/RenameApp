using System.Collections.Generic;

namespace RenameApp
{
    public class FilesNameComparerClass:IComparer<string>
    {
        ///<summary>
        ///比较两个字符串，如果含用数字，则数字按数字的大小来比较。
        ///</summary>
        ///<param name="x"></param>
        ///<param name="y"></param>
        ///<returns></returns>
        int IComparer<string>.Compare(string x, string y)
        {
            if(x == null || y == null)
                return x==null?1:-1;
            string fileA = x as string;
            string fileB = y as string;
            char[] arr1 = fileA.ToCharArray();
            char[] arr2 = fileB.ToCharArray();
            int i = 0, j = 0;
            bool errorCatch = false;
            while(i < arr1.Length && j < arr2.Length)
            {
                if(!errorCatch && char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                {
                    int s1 = 0, s2 = 0;
                    int temp = 0;

                    while(i < arr1.Length && char.IsDigit(arr1[i]))
                    {
                        try
                        {
                            temp = int.Parse(arr1[i].ToString());
                            s1 = s1 * 10 + temp;
                            i++;
                        }
                        catch
                        {
                            System.Console.WriteLine("fileA --> " + i + " : " + fileA);
                            errorCatch = true;
                            break;
                        }
                    }
                    while(j < arr2.Length && char.IsDigit(arr2[j]))
                    {
                        try
                        {
                            temp = int.Parse(arr2[j].ToString());
                            s2 = s2 * 10 + temp;
                            j++;
                        }
                        catch
                        {
                            System.Console.WriteLine("fileB --> " + j + " : " + fileB);
                            errorCatch = true;
                            break;
                        }
                    }
                    if(s1 > s2)
                    {
                        return 1;
                    }
                    if(s1 < s2)
                    {
                        return -1;
                    }
                }
                else
                {
                    if(arr1[i] > arr2[j])
                    {
                        return 1;
                    }
                    if(arr1[i] < arr2[j])
                    {
                        return -1;
                    }
                    errorCatch = false;
                    i++;
                    j++;
                }
            }
            if(arr1.Length == arr2.Length)
            {
                return 0;
            }
            else
            {
                return arr1.Length > arr2.Length ? 1 : -1;
            }
        }
    }
}
