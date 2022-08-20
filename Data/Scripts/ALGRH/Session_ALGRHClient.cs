// ;
using System;
using System.Collections.Generic;
using ExShared;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace ALGRH
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public partial class Session_ALGRHClient : MySessionComponentBase
    {
        public override void LoadData()
        {
            m_Logger = new Logger("client", "ALGRH");
            m_Logger.LogLevel = 4;

            m_IsServer = MyAPIGateway.Multiplayer.IsServer;
            m_IsDedicated = m_IsServer && MyAPIGateway.Utilities.IsDedicated;

            m_Logger.WriteLine("IsServer = " + m_IsServer);
            m_Logger.WriteLine("IsDedicated = " + m_IsDedicated);

            if (m_IsDedicated)
                return;

            MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControls_CustomControlGetter;
        }

        protected override void UnloadData()
        {
            if (!m_IsDedicated)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter -= TerminalControls_CustomControlGetter;
            }

            m_Logger.Close();
        }

        private void TerminalControls_CustomControlGetter(IMyTerminalBlock _block, List<IMyTerminalControl> _controls)
        {
            if (!(_block is IMyMotorAdvancedStator) || _block.CubeGrid.GridSizeEnum == MyCubeSize.Large)
                return;

            if (!m_IsControlCreated)
            {
                m_Logger.WriteLine("Creating control..");

                button = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyMotorAdvancedStator>("AddLargeGridRotorHeadBtn");
                button.Enabled = (_blk) => { return (_blk as IMyMotorAdvancedStator).Top == null; };
                button.Visible = (_blk) => { return true; };
                button.Action = AddLargeGridRotorHeadBtn_OnClicked;

                button.Title = MyStringId.GetOrCompute("Add Large Grid Head");
                button.Tooltip = MyStringId.GetOrCompute("Adds a Large Grid rotor head (not Large rotor head)");

                button.SupportsMultipleBlocks = true;

                m_IsControlCreated = true;
                m_Logger.WriteLine("Control created.");
            }

            m_Logger.WriteLine("Adding Control..", 5);
            for (int i = 0; i < _controls.Count; ++i)
            {
                // Id is taken from MyMotorStator;
                if (_controls[i].Id == "AddRotorTopPart")
                {
                    _controls.Insert(i, button);
                    m_Logger.WriteLine("  Insert button at " + i, 5);
                    break;
                }
            }
        }

        private void AddLargeGridRotorHeadBtn_OnClicked(IMyTerminalBlock _block)
        {
            m_Logger.WriteLine("Button Clicked");
            m_Logger.IndentIn();

            m_Logger.WriteLine("Sending request to server..");

            byte[] entityIdBytes = BitConverter.GetBytes(_block.EntityId);
            byte[] buffer = new byte[9];
            buffer[0] = 0xFA;
            for (int i = 1; i < 9; ++i)
            {
                buffer[i] = entityIdBytes[i - 1];
            }
            m_Logger.WriteLine(string.Format("SAN check: 0x{0:X02}:{1:0}", buffer[0], BitConverter.ToInt64(buffer, 1)), 5);

            MyAPIGateway.Multiplayer.SendMessageToServer(Constants.SYNC_TO_SERVER_ID, buffer);

            m_Logger.WriteLine("<< Done.");

            m_Logger.IndentOut();
        }

        private bool m_IsServer = false;
        private bool m_IsDedicated = false;
        private bool m_IsControlCreated = false;

        private Logger m_Logger = null;

        private IMyTerminalControlButton button = null;
    }

}