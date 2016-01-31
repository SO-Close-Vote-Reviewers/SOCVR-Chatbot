﻿/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.ChatbotActions;

namespace CVChatbot.Bot
{
    internal class SimilarCommand
    {
        private Regex cmdOptions = new Regex(@"[()<>\[\]].*?[()<>\[\]]");

        public KeyValuePair<string, ChatbotAction>? FindCommand(string message, double threshold = 2D / 3)
        {
            if (string.IsNullOrWhiteSpace(message)) return null;

            var y = new string(message.Where(c => char.IsLetter(c) || c == ' ').ToArray())
                .Trim()
                .ToLowerInvariant();
            var cmdDist = double.MaxValue;
            ChatbotAction act = null;

            foreach (var cmd in ChatbotActionRegister.AllChatActions)
            {
                if (string.IsNullOrWhiteSpace(cmd.ActionUsage)) continue;

                var x = cmdOptions.Replace(cmd.ActionUsage, "").ToLowerInvariant().Trim();

                if (!string.IsNullOrWhiteSpace(x))
                {
                    var z = y;
                    var lenDiff = 1D;
                    if (z.Length > x.Length)
                    {
                        z = z.Substring(0, (int)Math.Round(x.Length + Math.Max(1, (y.Length - x.Length) / 5D)));
                        lenDiff = (double)y.Length / x.Length;
                    }

                    var dist = Calculate(x, z, int.MaxValue) * lenDiff;

                    if (dist < cmdDist)
                    {
                        act = cmd;
                        cmdDist = dist;
                    }
                }
            }

            if ((y.Length - cmdDist) / y.Length < threshold)
            {
                return null;
            }

            var dynCmdOpt = "";

            var msgWords = message.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var cmdWords = cmdOptions.Replace(act.ActionUsage, "").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var ignoreWords = new List<int>();
            for (var i = 0; i < msgWords.Length; i++)
            {
                var dist = double.MaxValue;

                for (var j = 0; j < cmdWords.Length; j++)
                {
                    var d = Calculate(msgWords[i], cmdWords[j], int.MaxValue);

                    if (d < dist)
                    {
                        dist = d;
                    }
                }

                if ((msgWords[i].Length - dist) / msgWords[i].Length > (threshold / 1.5))
                {
                    ignoreWords.Add(i);
                }
            }
            for (var i = 0; i < msgWords.Length; i++)
            {
                if (ignoreWords.Contains(i)) continue;

                dynCmdOpt += msgWords[i] + " ";
            }

            var cmdTxt = cmdOptions.Replace(act.ActionUsage, dynCmdOpt.Trim());

            return new KeyValuePair<string, ChatbotAction>(cmdTxt, act);
        }



        private static int Calculate(string x, string y, int threshold)
        {
            int length1 = x.Length;
            int length2 = y.Length;

            // Return trivial case - difference in string lengths exceeds threshold.
            if (Math.Abs(length1 - length2) > threshold) return int.MaxValue;

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref y, ref x);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) dCurrent[i] = i;

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++)
            {
                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++)
                {
                    int cost = x[im1] == y[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && x[im2] == y[jm1] && x[im1] == y[j - 2])
                    {
                        min = Math.Min(min, dMinus2[im2] + cost);
                    }

                    dCurrent[i] = min;

                    if (min < minDistance) minDistance = min;

                    im1++;
                    im2++;
                }

                jm1++;

                if (minDistance > threshold) return int.MaxValue;
            }

            int result = dCurrent[maxi];

            return (result > threshold) ? int.MaxValue : result;
        }

        private static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}