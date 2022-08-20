// ;
using ExShared;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;

namespace ALGRH
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class Session_ALGRHServer : MySessionComponentBase
    {
        public override void LoadData()
        {
            m_Logger = new Logger("server", "ALGRH");

            m_IsServer = MyAPIGateway.Multiplayer.IsServer;
            m_IsDedicated = m_IsServer && MyAPIGateway.Utilities.IsDedicated;

            m_Logger.WriteLine("IsServer = " + m_IsServer);
            m_Logger.WriteLine("IsDedicated = " + m_IsDedicated);

            if (!m_IsServer)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.SYNC_TO_SERVER_ID, ReceiveDataFromClient);
            m_Logger.WriteLine("SecureMessageHandler registered.");
        }

        protected override void UnloadData()
        {
            if (m_IsServer)
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.SYNC_TO_SERVER_ID, ReceiveDataFromClient);
                m_Logger.WriteLine("SecureMessageHandler unregistered.");
            }

            m_Logger.Close();
        }

        private void ReceiveDataFromClient(ushort _handlerId, byte[] _package, ulong _senderPlayerId, bool _sentMsg)
        {
            m_Logger.WriteLine("Processing message received from client..");
            m_Logger.IndentIn();

            if (_package.Length != 9 || _package[0] != 0xFA)
            {
                string s = string.Format("0x{0:X02}" + _package[0]);
                m_Logger.WriteLine("Invalid package (length = " + _package.Length + ", first byte = " + s + ").");
                return;
            }

            long entityId = BitConverter.ToInt64(_package, 1);
            m_Logger.WriteLine(string.Format("SAN check: 0x{0:X02}:{1:0}", _package[0], entityId), 5);

            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
            if (entity == null)
            {
                m_Logger.WriteLine("Entity (" + entityId + ") not found.");
                return;
            }

            IMyMotorAdvancedStator stator = (IMyMotorAdvancedStator)entity;
            if (stator == null)
            {
                m_Logger.WriteLine("Entity (" + entityId + ") is not stator.");
                return;
            }
            if (stator.CubeGrid.GridSizeEnum != MyCubeSize.Small)
            {
                m_Logger.WriteLine("Stator (" + entityId + ") is not small grid.");
                return;
            }

            m_Logger.WriteLine("Spawning rotor head per request..");
            m_Logger.IndentIn();
            SpawnHeadPerRequest(stator);
            m_Logger.IndentOut();

            m_Logger.WriteLine("<< Done.");
            m_Logger.IndentOut();
        }

        private void SpawnHeadPerRequest(IMyMotorAdvancedStator _stator)
        {
            if (m_StatorBlockDefinition == null)
            {
                m_StatorBlockDefinition = (MyMotorStatorDefinition)MyDefinitionManager.Static.GetDefinition(s_StatorBlockDefId);
                if (m_StatorBlockDefinition == null)
                {
                    m_Logger.WriteLine("Medium stator block definition is still null (this should not happen).");
                    return;
                }
            }

            if (m_RotorTopDefGroup == null)
            {
                m_RotorTopDefGroup = MyDefinitionManager.Static.GetDefinitionGroup(m_StatorBlockDefinition.TopPart);
                if (m_RotorTopDefGroup == null)
                {
                    m_Logger.WriteLine("Rotor top definition group is still null (this should not happen).");
                    return;
                }
            }

            m_Logger.WriteLine("Invoking spawn rotor head..");
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                m_Logger.WriteLine("Invoking..");
                m_Logger.IndentIn();

                MyCubeBlockDefinition orgDefLarge = m_RotorTopDefGroup[MyCubeSize.Large];
                m_RotorTopDefGroup[MyCubeSize.Large] = MyDefinitionManager.Static.GetCubeBlockDefinition(s_LargeRotorHeadDefId);

                string strOrgDefLarge;
                if (orgDefLarge == null)
                {
                    strOrgDefLarge = "null";
                }
                else
                {
                    strOrgDefLarge = orgDefLarge.Id.ToString();
                }
                m_Logger.WriteLine("Definition changed: " + strOrgDefLarge + " -> " + m_RotorTopDefGroup[MyCubeSize.Large].Id.ToString() + ".");

                MyCubeBlock block = (MyCubeBlock)_stator;
                bool creativeEnabled = MyAPIGateway.Session.CreativeMode && MyAPIGateway.Session.HasCreativeRights;

                block.CubeGrid.GridSizeEnum = MyCubeSize.Large;
                block.OnBuildSuccess(block.OwnerId, creativeEnabled);
                m_Logger.WriteLine("Spawned.");

                block.CubeGrid.GridSizeEnum = MyCubeSize.Small;
                m_RotorTopDefGroup[MyCubeSize.Large] = orgDefLarge;
                m_Logger.WriteLine("Definition restored.");

                m_Logger.WriteLine("<< Done.");
                m_Logger.IndentOut();
                
            });

            m_Logger.WriteLine("<< Done.");
        }




        private static MyDefinitionId s_StatorBlockDefId = MyDefinitionId.Parse("MyObjectBuilder_MotorAdvancedStator/SmallAdvancedStator");
        private static MyDefinitionId s_LargeRotorHeadDefId = MyDefinitionId.Parse("MyObjectBuilder_MotorAdvancedRotor/LargeAdvancedRotor");

        private bool m_IsServer = false;
        private bool m_IsDedicated = false;

        private Logger m_Logger = null;

        private MyMotorStatorDefinition m_StatorBlockDefinition = null;
        private MyCubeBlockDefinitionGroup m_RotorTopDefGroup = null;
    }

}
