using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    /// <summary>
    /// 冒泡排序
    /// </summary>
    public class BubbleSorter
    {
        public void BubbleSort(int[] array)
        {
            int i, j, temp; //交换标志
            bool exchange;
            for (i = 0; i < array.Length; i++) //最多做array.Length-1趟排序
            {
                exchange = false; //本趟排序开始前，交换标志应为假
                for (j = array.Length - 2; j >= i; j--)
                {
                    if (array[j + 1] < array[j]) //交换条件
                    {
                        temp = array[j + 1];
                        array[j + 1] = array[j];
                        array[j] = temp;
                        exchange = true; //发生了交换，故将交换标志置为真
                    }
                }
                if (!exchange) //本趟排序未发生交换，提前终止算法
                {
                    break;
                }
            }
        }

        public void BubbleSort(List<UrlCombination> list)
        {
            int i, j;
            UrlCombination temp; //交换标志
            bool exchange;
            for (i = 0; i < list.Count; i++) //最多做list.Count-1趟排序
            {
                exchange = false; //本趟排序开始前，交换标志应为假
                for (j = list.Count - 2; j >= i; j--)
                {
                    if (list[j + 1].SimHash < list[j].SimHash) //交换条件
                    {
                        temp = list[j + 1];
                        list[j + 1] = list[j];
                        list[j] = temp;
                        exchange = true; //发生了交换，故将交换标志置为真
                    }
                }
                if (!exchange) //本趟排序未发生交换，提前终止算法
                {
                    break;
                }
            }
        }
    }
}