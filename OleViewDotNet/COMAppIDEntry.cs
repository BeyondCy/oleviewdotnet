﻿//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Win32;
using System;
using System.ComponentModel;

namespace OleViewDotNet
{
    [Serializable]
    public class COMAppIDEntry : IComparable<COMAppIDEntry>
    {     
        private byte[] m_access;
        private byte[] m_launch;

        public COMAppIDEntry(Guid appId, RegistryKey key)
        {
            AppId = appId;
            LoadFromKey(key);
        }

        private void LoadFromKey(RegistryKey key)
        {
            LocalService = key.GetValue("LocalService") as string;
            RunAs = key.GetValue("RunAs") as string;
            string name = key.GetValue(null) as string;
            if (!String.IsNullOrWhiteSpace(name))
            {
                Name = name.ToString();
            }
            else
            {
                Name = AppId.ToString("B");
            }

            m_access = key.GetValue("AccessPermission") as byte[];
            m_launch = key.GetValue("LaunchPermission") as byte[];
            DllSurrogate = key.GetValue("DllSurrogate") as string;
            if (DllSurrogate != null)
            {
                if (String.IsNullOrWhiteSpace(DllSurrogate))
                {
                    DllSurrogate = "dllhost.exe";
                }
                else
                {
                    string dllexe = key.GetValue("DllSurrogateExecutable") as string;
                    if (!String.IsNullOrWhiteSpace(dllexe))
                    {
                        DllSurrogate = dllexe;
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(RunAs) && !String.IsNullOrWhiteSpace(LocalService))
            {
                using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\" + LocalService))
                {
                    if (serviceKey != null)
                    {
                        RunAs = serviceKey.GetValue("ObjectName") as string;
                    }
                }
            }
        }

        public int CompareTo(COMAppIDEntry other)
        {
            return AppId.CompareTo(other.AppId);
        }

        public Guid AppId { get; private set; }        

        public string DllSurrogate { get; private set; }

        public string LocalService { get; private set; }

        public string RunAs { get; private set; }

        public string Name { get; private set; }

        public byte[] LaunchPermission
        {
            get { return m_launch; }
        }

        public byte[] AccessPermission
        {
            get { return m_access; }
        }

        public string LaunchPermissionString
        {
            get
            {
                if ((m_launch != null) && (m_launch.Length > 0))
                {
                    try
                    {
                        return COMUtilities.GetStringSDForSD(m_launch);
                    }
                    catch (Win32Exception)
                    {
                    }
                }

                return null;
            }
        }

        public string AccessPermissionString
        {
            get
            {
                if ((m_access != null) && (m_access.Length > 0))
                {
                    try
                    {
                        return COMUtilities.GetStringSDForSD(m_access);
                    }
                    catch (Win32Exception)
                    {
                    }
                }

                return null;
            }
        }

        public override string ToString()
        {
            return String.Format("COMAppIDEntry: {0}", Name);
        }
    }
}
