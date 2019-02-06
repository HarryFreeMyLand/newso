﻿using FSO.Client.UI.Framework;
using FSO.Files.Formats.IFF.Chunks;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.IDE.EditorComponent.Model;
using Microsoft.Xna.Framework.Graphics;
using FSO.Client.UI.Controls;
using FSO.IDE.EditorComponent.Commands;
using Microsoft.Xna.Framework.Input;
using FSO.SimAntics;
using FSO.SimAntics.Engine;
using FSO.IDE.EditorComponent.DataView;

namespace FSO.IDE.EditorComponent.UI
{
    public class UIBHAVEditor : UIContainer
    {
        public BHAVContainer BHAVView;

        private List<BHAVCommand> Commands = new List<BHAVCommand>();
        private List<VMModifyDataCommand> ValueChangeCmds = new List<VMModifyDataCommand>();
        private Stack<BHAVCommand> UndoStack = new Stack<BHAVCommand>();
        private Stack<BHAVCommand> RedoStack = new Stack<BHAVCommand>();

        private PrimitiveBox Placement;

        public int UndoRedoDir;

        private UILabel PlacingName;
        private UILabel PlacingDesc;

        private int CutoutPhase;

        private int LastWidth;
        private int LastHeight;

        private bool MouseWasDown;
        private bool RightMouseWasDown;
        private bool RedrawNext;

        private bool DebugMode;
        private VMEntity DebugEntity;
        public VMStackFrame DebugFrame;

        public event BHAVEditor.DisableDebuggerDelegate DisableDebugger;

        //Debug only buttons;
        private UIButton DebugGo;
        private UIButton DebugStepOver;
        private UIButton DebugStepIn;
        private UIButton DebugStepOut;
        private UIButton DebugTrue;
        private UIButton DebugFalse;

        private Dictionary<ushort, BHAVContainer> ContainerByID;

        public UIBHAVEditor(BHAV target, EditorScope scope, VMEntity debugEnt)
        {
            if (debugEnt != null)
            {
                DebugMode = true;
                DebugEntity = debugEnt;
            }

            ContainerByID = new Dictionary<ushort, BHAVContainer>();
            BHAVView = new BHAVContainer(target, scope);
            ContainerByID.Add(target.ChunkID, BHAVView);
            this.Add(BHAVView);

            PlacingName = new UILabel();
            PlacingName.Alignment = TextAlignment.Center;
            PlacingName.Size = new Vector2(1, 1);
            PlacingName.CaptionStyle = TextStyle.DefaultLabel.Clone();
            PlacingName.CaptionStyle.Font = FSO.Client.GameFacade.EdithFont;
            PlacingName.CaptionStyle.Size = 14;
            PlacingName.CaptionStyle.Color = new Color(0, 102, 26);

            PlacingName.Caption = "Placing Report Metric";

            PlacingDesc = new UILabel();
            PlacingDesc.Alignment = TextAlignment.Center;
            PlacingDesc.Size = new Vector2(1, 1);
            PlacingDesc.CaptionStyle = TextStyle.DefaultLabel.Clone();
            PlacingDesc.CaptionStyle.Font = FSO.Client.GameFacade.EdithFont;
            PlacingDesc.CaptionStyle.Size = 12;
            PlacingDesc.CaptionStyle.Color = new Color(0, 102, 26);

            PlacingDesc.Caption = "Press ESC to cancel.";

            this.Add(PlacingName);
            this.Add(PlacingDesc);

            if (DebugMode)
            {
                DebugFrame = debugEnt.Thread.Stack.LastOrDefault();
                UpdateDebugPointer(DebugFrame);
                DebugGo = new UIButton();
                DebugGo.Caption = "Go";
                DebugGo.Position = new Vector2(15, 15);
                Add(DebugGo);
                DebugGo.OnButtonClick += DebugButtonClick;

                DebugStepOver = new UIButton();
                DebugStepOver.Caption = "Step Over";
                DebugStepOver.Position = new Vector2(83, 15);
                Add(DebugStepOver);
                DebugStepOver.OnButtonClick += DebugButtonClick;

                DebugStepIn = new UIButton();
                DebugStepIn.Caption = "Step In";
                DebugStepIn.Position = new Vector2(193, 15);
                Add(DebugStepIn);
                DebugStepIn.OnButtonClick += DebugButtonClick;

                DebugStepOut = new UIButton();
                DebugStepOut.Caption = "Step Out";
                DebugStepOut.Position = new Vector2(293, 15);
                Add(DebugStepOut);
                DebugStepOut.OnButtonClick += DebugButtonClick;

                DebugTrue = new UIButton();
                DebugTrue.Caption = "Return True";
                DebugTrue.Position = new Vector2(402, 15);
                Add(DebugTrue);
                DebugTrue.OnButtonClick += DebugButtonClick;

                DebugFalse = new UIButton();
                DebugFalse.Caption = "Return False";
                DebugFalse.Position = new Vector2(522, 15);
                Add(DebugFalse);
                DebugFalse.OnButtonClick += DebugButtonClick;
            }
        }

        private void DebugButtonClick(UIElement button)
        {
            if (button == DebugGo)
                if (DebugEntity.Thread.ThreadBreak == VMThreadBreakMode.Active)
                    DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.Immediate;
                else
                    DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.Active;
            else if (button == DebugStepIn)
                DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.StepIn;
            else if (button == DebugStepOver)
                DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.StepOver;
            else if (button == DebugStepOut)
                DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.StepOut;
            else if (button == DebugTrue)
                DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.ReturnTrue;
            else if (button == DebugFalse)
                DebugEntity.Thread.ThreadBreak = VMThreadBreakMode.ReturnFalse;
            else return;
        }

        public void Resume()
        {
            DebugGo.Caption = "Pause";
            DebugStepIn.Disabled = true;
            DebugStepOut.Disabled = true;
            DebugStepOver.Disabled = true;
            DebugTrue.Disabled = true;
            DebugFalse.Disabled = true;
            if (DisableDebugger != null) DisableDebugger();
            BHAVView.DebugPointer = null;
        }

        public void NewBreak(VMStackFrame frame)
        {
            DebugGo.Caption = "Go";
            DebugStepIn.Disabled = false;
            DebugStepOut.Disabled = false;
            DebugStepOver.Disabled = false;
            DebugTrue.Disabled = false;
            DebugFalse.Disabled = false;
            RedrawNext = true;
            DebugFrame = frame;
            UpdateDebugPointer(DebugFrame);
        }

        public void UpdateDebugPointer(VMStackFrame frame)
        {
            if (frame != null && BHAVView.EditTarget.ChunkID == frame.Routine.ID)
            {
                BHAVView.DebugPointer = BHAVView.RealPrim[frame.InstructionPointer];
            }
            else
            {
                BHAVView.DebugPointer = null;
            }
        }

        public void SwitchBHAV(BHAV target, EditorScope scope, VMStackFrame frame)
        {
            Remove(BHAVView);
            if (ContainerByID.ContainsKey(target.ChunkID))
            {
                BHAVView = ContainerByID[target.ChunkID];
                AddAt(0, BHAVView);
            } else
            {
                BHAVView = new BHAVContainer(target, scope);
                ContainerByID.Add(target.ChunkID, BHAVView);
                AddAt(0, BHAVView);
            }
            if (DebugMode)
            {
                DebugFrame = frame;
                UpdateDebugPointer(frame);
            }
        }

        public override void Update(UpdateState state)
        {
            lock (Commands)
            {
                if (Commands.Count > 0) RedoStack.Clear();
                foreach(var cmd in Commands)
                {
                    state.SharedData["ExternalDraw"] = true;
                    cmd.Execute(BHAVView.EditTarget, this);
                    UndoStack.Push(cmd);
                }
                Commands.Clear();
            }

            lock (ValueChangeCmds)
            {
                foreach (var cmd in ValueChangeCmds)
                {
                    cmd.Execute();
                }
                Commands.Clear();
            }

            while (UndoRedoDir > 0)
            {
                if (UndoStack.Count > 0)
                {
                    var cmd = UndoStack.Pop();
                    state.SharedData["ExternalDraw"] = true;
                    cmd.Undo(BHAVView.EditTarget, this);
                    RedoStack.Push(cmd);
                }
                UndoRedoDir--;
            }

            while (UndoRedoDir < 0)
            {
                if (RedoStack.Count > 0)
                {
                    var cmd = RedoStack.Pop();
                    state.SharedData["ExternalDraw"] = true;
                    cmd.Execute(BHAVView.EditTarget, this);
                    UndoStack.Push(cmd);
                }
                UndoRedoDir++;
            }

            if (RedrawNext)
            {
                state.SharedData["ExternalDraw"] = true;
                RedrawNext = false;
            }

            if (Placement != null)
            {
                Placement.Position = GlobalPoint(new Vector2(state.MouseState.X, state.MouseState.Y)) - (new Vector2(Placement.Width, Placement.Height) / 2);
                Placement.Style = PGroupStyles.ByType[PrimitiveGroup.Placement];
                state.SharedData["ExternalDraw"] = true;
                Placement.Update(state);

                PlacingName.Position = new Vector2(LastWidth / 2, LastHeight - 66);
                PlacingDesc.Position = new Vector2(LastWidth / 2, LastHeight - 48);

                var mx = state.MouseState.Position.X;
                var my = state.MouseState.Position.Y;

                if (MouseWasDown && (state.MouseState.LeftButton == ButtonState.Released)
                    && mx > 0 && mx < LastWidth && my > 0 && my < LastHeight)
                {
                    QueueCommand(new AddPrimCommand(Placement));
                    Placement.Position -= BHAVView.Position;
                    ClearPlacement();
                }
            }
            CutoutPhase++;
            MouseWasDown = state.MouseState.LeftButton == ButtonState.Pressed;
            base.Update(state);

            if (BHAVView.HoverPrim != null && (!RightMouseWasDown) && 
                state.MouseState.RightButton == ButtonState.Pressed
                && BHAVView.HoverPrim.Type == PrimBoxType.Primitive)
            {
                QueueCommand(new ToggleBreakpointCommand(BHAVView.HoverPrim));
            }

            RightMouseWasDown = state.MouseState.RightButton == ButtonState.Pressed;
        }

        public void UpdateOperand(PrimitiveBox target)
        {
            var newOp = new byte[8];
            target.Descriptor.Operand.Write(newOp);

            QueueCommand(new OpModifyCommand(target, newOp));
        }

        public void QueueCommand(BHAVCommand cmd)
        {
            lock (Commands)
            {
                Commands.Add(cmd);
            }
        }

        internal void QueueValueChange(VMModifyDataCommand cmd)
        {
            lock (ValueChangeCmds)
            {
                ValueChangeCmds.Add(cmd);
            }
        }


        public void SetPlacement(ushort primType)
        {
            PlacingName.Visible = true;
            PlacingDesc.Visible = true;
            
            if (primType == 254 || primType == 255)
            {
                Placement = new PrimitiveBox((primType == 254) ? PrimBoxType.True : PrimBoxType.False, BHAVView);
                PlacingName.Caption = "Placing Return " + ((primType == 254) ? "True" : "False");
            }
            else
            {
                Placement = new PrimitiveBox(new BHAVInstruction
                {
                    TruePointer = 253,
                    FalsePointer = 253,
                    Opcode = primType,
                    Operand = new byte[8]
                }, 255, BHAVView);
                PlacingName.Caption = "Placing " + Placement.TitleText;
            }
            Placement.Parent = this;
        }
        public void ClearPlacement()
        {
            PlacingName.Visible = false;
            PlacingDesc.Visible = false;
            Placement = null;
            RedrawNext = true;
        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
            if (Placement != null)
            {
                Placement.ShadDraw(batch);
                Placement.Draw(batch);
            }
            var res = EditorResource.Get();
            DrawLocalTexture(batch, res.WhiteTex, null, new Vector2(), new Vector2(4, batch.Height), Color.Black * 0.2f);
            DrawLocalTexture(batch, res.WhiteTex, null, new Vector2(4, 0), new Vector2(batch.Width, 4), Color.Black * 0.2f);

            if (Placement != null)
            {
                DrawCutoutLines(CutoutPhase, 5, Color.Black * 0.2f, batch);
                DrawCutoutLines(CutoutPhase, 0, new Color(0, 102, 26), batch);
            }

            LastWidth = batch.Width;
            LastHeight = batch.Height;
        }

        public void DrawCutoutLines(int phase, int offset, Color color, UISpriteBatch batch)
        {
            var res = EditorResource.Get();
            int margin = 24;

            int boxWidth = batch.Width - margin * 2;
            int boxHeight = batch.Height - margin * 2;

            int i = phase%32;
            bool draw = ((phase/32)%2) == 1;
            i -= 32;
            while (i < boxWidth)
            {
                if (draw)
                {
                    DrawLine(res.WhiteTex,
                    new Vector2(Math.Max(margin, margin+i) + offset, margin + offset),
                    new Vector2(Math.Min(batch.Width - margin, margin + i + 32) + offset, margin + offset),
                    batch, 4, color);
                }

                i += 32; draw = !draw;
            }
            i -= boxWidth + 32;
            draw = !draw;

            while (i < boxHeight)
            {
                if (draw)
                {
                    DrawLine(res.WhiteTex,
                    new Vector2(offset + batch.Width - margin, Math.Max(margin, margin + i) + offset),
                    new Vector2(offset + batch.Width - margin, Math.Min(batch.Height - margin, margin + i + 32) + offset),
                    batch, 4, color);
                }

                i += 32; draw = !draw;
            }
            i -= boxHeight + 32;
            draw = !draw;

            while (i < boxWidth)
            {
                if (draw)
                {
                    DrawLine(res.WhiteTex,
                    new Vector2(batch.Width-Math.Max(margin, margin + i) + offset, (batch.Height-margin) + offset),
                    new Vector2(batch.Width-Math.Min(batch.Width - margin, margin + i + 32) + offset, (batch.Height - margin) + offset),
                    batch, 4, color);
                }

                i += 32; draw = !draw;
            }
            i -= boxWidth + 32;
            draw = !draw;

            while (i < boxHeight)
            {
                if (draw)
                {
                    DrawLine(res.WhiteTex,
                    new Vector2(offset + margin, batch.Height-Math.Max(margin, margin + i) + offset),
                    new Vector2(offset + margin, batch.Height-Math.Min(batch.Height - margin, margin + i + 32) + offset),
                    batch, 4, color);
                }

                i += 32; draw = !draw;
            }

        }

        private void DrawLine(Texture2D Fill, Vector2 Start, Vector2 End, SpriteBatch spriteBatch, int lineWidth, Color tint) //draws a line from Start to End.
        {
            Start = LocalPoint(Start);
            End = LocalPoint(End);
            Start.Y += lineWidth / 2;
            End.Y += lineWidth / 2;
            double length = Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));
            float direction = (float)Math.Atan2(End.Y - Start.Y, End.X - Start.X);
            spriteBatch.Draw(Fill, new Rectangle((int)Start.X, (int)Start.Y - (int)(lineWidth / 2), (int)length, lineWidth), null, tint, direction, new Vector2(0, 0.5f), SpriteEffects.None, 0); //
        }

    }
}
