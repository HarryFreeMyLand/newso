﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Client.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FSO.Client.UI.Framework
{
    public class TextRenderer
    {
        public static void DrawText(List<ITextDrawCmd> cmds, UIElement target, SpriteBatch batch)
        {
            /**
             * Draw text
             */
            foreach (var cmd in cmds)
            {
                cmd.Draw(target, batch);
            }
        }

        /// <summary>
        /// Computes drawing commands to layout a block of text within
        /// certain constraints
        /// </summary>
        /// <returns></returns>
        public static TextRendererResult ComputeText(string text, TextRendererOptions options, UIElement target)
        {
            var TextStyle = options.TextStyle;
            var _Scale = options.Scale;
            var txtScale = TextStyle.Scale * _Scale;

            var m_LineHeight = TextStyle.MeasureString("W").Y - (2 * txtScale.Y);
            var spaceWidth = TextStyle.MeasureString(" ").X;

            var words = text.Split(' ').ToList();
            var newWordsArray = TextRenderer.ExtractLineBreaks(words);

            var m_Lines = new List<UITextEditLine>();
            TextRenderer.CalculateLines(m_Lines, newWordsArray, TextStyle, options.MaxWidth, spaceWidth, options.TopLeftIconSpace, m_LineHeight);

            var topLeft = options.Position;
            var position = topLeft;

            var result = new TextRendererResult();
            var drawCommands = new List<ITextDrawCmd>();
            result.DrawingCommands = drawCommands;

            var yPosition = topLeft.Y;
            var numLinesAdded = 0;
            var realMaxWidth = 0;
            for (var i = 0; i < m_Lines.Count; i++)
            {
                var lineOffset = (i*m_LineHeight < options.TopLeftIconSpace.Y) ? options.TopLeftIconSpace.X : 0;
                var line = m_Lines[i];
                var xPosition = topLeft.X+lineOffset;

                if (line.LineWidth > realMaxWidth) realMaxWidth = (int)line.LineWidth;

                /** Alignment **/
                if (options.Alignment == TextAlignment.Center)
                {
                    xPosition += (int)Math.Round(((options.MaxWidth-lineOffset) - line.LineWidth) / 2);
                }

                var segmentPosition = target.LocalPoint(new Vector2(xPosition, yPosition));
                drawCommands.Add(new TextDrawCmd_Text
                {
                    Selected = false,
                    Text = line.Text,
                    Style = TextStyle,
                    Position = segmentPosition,
                    Scale = txtScale
                });
                numLinesAdded++;


                yPosition += m_LineHeight;
                position.Y += m_LineHeight;
            }

            result.BoundingBox = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)options.MaxWidth, (int)(yPosition-m_LineHeight));
            result.MaxWidth = realMaxWidth;
            foreach (var cmd in drawCommands)
            {
                cmd.Init();
            }

            return result;
        }

        public static void CalculateLines(List<UITextEditLine> m_Lines, List<string> newWordsArray, TextStyle TextStyle, float lineWidth, float spaceWidth, Vector2 topLeftIconSpace, float lineHeight)
        {
            var currentLine = new StringBuilder();
            var currentLineWidth = 0.0f;
            var currentLineNum = 0;

            for (var i = 0; i < newWordsArray.Count; i++)
            {
                var allowedWidth = (currentLineNum*lineHeight<topLeftIconSpace.Y)?lineWidth-topLeftIconSpace.X:lineWidth;
                var word = newWordsArray[i];

                if (word == "\r\n")
                {
                    /** Line break **/
                    m_Lines.Add(new UITextEditLine
                    {
                        Text = currentLine.ToString(),
                        LineWidth = currentLineWidth,
                        LineNumber = currentLineNum,
                        WhitespaceSuffix = 2
                    });
                    currentLineNum++;
                    currentLine = new StringBuilder();

                    currentLineWidth = 0;
                }
                else
                {
                    bool wordWritten = false;
                    while (!wordWritten) //repeat until the full word is written (as part of it can be written each pass if it is too long)
                    {
                        var wordSize = TextStyle.MeasureString(word);

                        if (wordSize.X > allowedWidth)
                        {
                            //SPECIAL CASE, word is bigger than line width and cannot fit on its own line
                            if (currentLineWidth > 0)
                            {
                                //if there are words on this line, we'll start this one on the next to get the most space for it
                                m_Lines.Add(new UITextEditLine
                                {
                                    Text = currentLine.ToString(),
                                    LineWidth = currentLineWidth,
                                    LineNumber = currentLineNum
                                });
                                currentLineNum++;
                                currentLine = new StringBuilder();
                                currentLineWidth = 0;
                            }

                            // binary search, makes this a bit faster?
                            // we can safely say that no character is thinner than 4px, so set max substring to maxwidth/4
                            float width = allowedWidth + 1;
                            int min = 1;
                            int max = Math.Min(word.Length, (int)allowedWidth / 4);
                            int mid = (min + max) / 2;
                            while (max-min > 1)
                            {
                                width = TextStyle.MeasureString(word.Substring(0, mid)).X;                    
                                if (width > allowedWidth)
                                    max = mid;
                                else
                                    min = mid;
                                mid = (max + min) / 2;
                            }
                            currentLine.Append(word.Substring(0, min));
                            currentLineWidth += width;
                            word = word.Substring(min);

                            m_Lines.Add(new UITextEditLine
                            {
                                Text = currentLine.ToString(),
                                LineWidth = currentLineWidth,
                                LineNumber = currentLineNum,
                                WhitespaceSuffix = 1
                            });

                            currentLineNum++;
                            currentLine = new StringBuilder();
                            currentLineWidth = 0;
                        }
                        else if (currentLineWidth + wordSize.X < allowedWidth)
                        {
                            currentLine.Append(word);
                            if (i != newWordsArray.Count - 1) { currentLine.Append(' '); currentLineWidth += spaceWidth; }
                            currentLineWidth += wordSize.X;
                            wordWritten = true;
                        }
                        else
                        {
                            /** New line **/
                            m_Lines.Add(new UITextEditLine
                            {
                                Text = currentLine.ToString(),
                                LineWidth = currentLineWidth,
                                LineNumber = currentLineNum,
                                WhitespaceSuffix = 1
                            });
                            currentLineNum++;
                            currentLine = new StringBuilder();
                            currentLine.Append(word);
                            currentLineWidth = wordSize.X;
                            if (i != newWordsArray.Count - 1) { currentLine.Append(' '); currentLineWidth += spaceWidth; }
                            wordWritten = true;
                        }
                    }
                }
            }

            m_Lines.Add(new UITextEditLine //add even if length is 0, so we can move the cursor down!
            {
                Text = currentLine.ToString(),
                LineWidth = currentLineWidth,
                LineNumber = currentLineNum
            });

            var currentIndex = 0;
            foreach (var line in m_Lines)
            {
                line.StartIndex = currentIndex;
                currentIndex += (line.Text.Length - 1) + line.WhitespaceSuffix;
            }
        }

        public static List<string> ExtractLineBreaks(List<string> words)
        {
            /**
             * Modify the array to make manual line breaks their own segment
             * in the array
             */
            var newWordsArray = new List<string>();
            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];
                var breaks = word.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                for (var x = 0; x < breaks.Length; x++)
                {
                    newWordsArray.Add(breaks[x]);
                    if (x != breaks.Length - 1)
                    {
                        newWordsArray.Add("\r\n");
                    }
                }
            }

            return newWordsArray;
        }
    }

    public class TextRendererResult
    {
        public List<ITextDrawCmd> DrawingCommands;
        public Rectangle BoundingBox;
        public int MaxWidth;
        public int Lines;
    }

    public class TextRendererOptions
    {
        public bool WordWrap;
        public int MaxWidth;
        public TextStyle TextStyle;
        public Vector2 Position;
        public Vector2 Scale;
        public TextAlignment Alignment;
        public Vector2 TopLeftIconSpace; //space to wrap around where an icon should be.
    }
}