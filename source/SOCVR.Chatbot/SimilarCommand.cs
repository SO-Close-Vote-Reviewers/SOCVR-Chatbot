using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SOCVR.Chatbot.ChatbotActions;
using SOCVR.Chatbot.ChatbotActions.Commands;

namespace CVChatbot.Bot
{
    internal class SimilarCommand
    {
        public class Results
        {
            public string SuggestedCmdText { get; set; }
            public ChatbotAction SuggestedAction { get; set; }
            public bool OptionsSubstituted { get; set; }
        }

        private Regex cmdOptionsReg = new Regex(@"[()<>\[\]].*?[()<>\[\]]");



        public Results FindCommand(string message, double threshold = 2D / 3)
        {
            if (string.IsNullOrWhiteSpace(message)) return null;

            var y = message.ToLowerInvariant();
            var z = y.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var cmds = new Dictionary<UserCommand, double>();

            foreach (var cmd in ChatbotActionRegister.AllUserCommands)
            {
                if (string.IsNullOrWhiteSpace(cmd.ActionUsage)) continue;

                var x = cmdOptionsReg
                    .Replace(cmd.ActionUsage, "")
                    .ToLowerInvariant()
                    .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                cmds[cmd] = 0;

                foreach (var aw in x)
                {
                    var d = double.MaxValue;

                    foreach (var mw in z)
                    {
                        var lenDiff = 1D;

                        if (mw.Length > aw.Length)
                        {
                            lenDiff = (double)mw.Length / aw.Length;
                        }
                        else
                        {
                            lenDiff = (double)aw.Length / mw.Length;
                        }

                        var dist = Calculate(aw, mw, int.MaxValue) * lenDiff;

                        if (dist < d)
                        {
                            d = dist;
                        }
                    }

                    cmds[cmd] += d;
                }
            }

            var min = cmds.Values.Min();
            var act = cmds.First(x => x.Value == min).Key;
            var actLen = cmdOptionsReg
                .Replace(act.ActionUsage, "")
                .Replace("  ", " ")
                .Trim()
                .Length;
            if ((actLen - min) / actLen < threshold)
            {
                return null;
            }

            var cmdTxt = GetCommandText(message, act, threshold);

            // If the chat command args are incorrect, show the generic
            // command usage instead.
            var optsSub = true;
            if (!act.DoesChatMessageActiveAction(cmdTxt, true))
            {
                cmdTxt = act.ActionUsage;
                optsSub = false;
            }

            return new Results
            {
                SuggestedCmdText = cmdTxt,
                SuggestedAction = act,
                OptionsSubstituted = optsSub
            };
        }



        private string GetCommandText(string message, UserCommand act, double threshold)
        {
            var cmdOpts = new List<List<string>>();
            var msgWords = message.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var cmdWords = cmdOptionsReg
                .Replace(act.ActionUsage, "")
                .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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
                    cmdOpts.Add(new List<string>());
                }
            }

            if (ignoreWords.Count == msgWords.Length)
            {
                return act.ActionUsage;
            }

            var curCmdOptsInd = 0;
            for (var i = 0; i < msgWords.Length; i++)
            {
                if (ignoreWords.Contains(i))
                {
                    if (ignoreWords[0] != i) curCmdOptsInd++;

                    continue;
                }
                cmdOpts[curCmdOptsInd].Add(msgWords[i]);
            }
            cmdOpts = cmdOpts.Where(x => x.Count > 0).ToList();

            var ms = cmdOptionsReg.Matches(act.ActionUsage);
            var cmdTxt = act.ActionUsage;
            var globalDiff = 0;
            var cmdOptI = 0;
            foreach (Match m in ms)
            {
                if (cmdOptI >= cmdOpts.Count)
                {
                    cmdTxt = cmdOptionsReg.Replace(cmdTxt, "");
                    break;
                }

                var localDiff = 0;
                cmdTxt = cmdTxt.Remove(m.Index + globalDiff, m.Length);
                foreach (var opt in cmdOpts[cmdOptI])
                {
                    cmdTxt = cmdTxt.Insert(m.Index + localDiff + globalDiff, opt + " ");
                    localDiff += opt.Length + 1;
                }
                globalDiff += localDiff - m.Length;
                cmdOptI++;
            }

            return cmdTxt.Replace("  ", " ").Trim();
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