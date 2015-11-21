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
            string fileA = (x as string).ToLower();
            string fileB = (y as string).ToLower();
            char[] arr1 = fileA.ToCharArray();
            char[] arr2 = fileB.ToCharArray();
            int i = 0, j = 0;
            while(i < arr1.Length && j < arr2.Length)
            {
                if(char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                {
                    string s1 = "", s2 = "";
                    while(i < arr1.Length && char.IsDigit(arr1[i]))
                    {
                        s1 += arr1[i];
                        i++;
                    }
                    while(j < arr2.Length && char.IsDigit(arr2[j]))
                    {
                        s2 += arr2[j];
                        j++;
                    }
                    if(int.Parse(s1) > int.Parse(s2))
                    {
                        return 1;
                    }
                    if(int.Parse(s1) < int.Parse(s2))
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
