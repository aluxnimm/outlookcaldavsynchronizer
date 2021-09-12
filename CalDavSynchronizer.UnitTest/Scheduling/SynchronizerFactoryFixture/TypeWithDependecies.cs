// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Contacts;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using Thought.vCards;

namespace CalDavSynchronizer.UnitTest.Scheduling.SynchronizerFactoryFixture
{
    public class TypeWithDependecies
    {
        public Type Type { get; }
        public IReadOnlyList<TypeWithDependecies> Dependencies { get; }
        public int InstanceId { get; }

        public TypeWithDependecies(Type type, IReadOnlyList<TypeWithDependecies> dependencies, int instanceId)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            Type = type;
            Dependencies = dependencies;
            InstanceId = instanceId;
        }

        public void ToString(StringBuilder stringBuilder, int level = 0)
        {
            stringBuilder.Append(string.Join(string.Empty, Enumerable.Range(1, level).Select(_ => "|   ")));
            stringBuilder.Append(Type.GetPrettyName());
            if (InstanceId > 1)
            {
                stringBuilder.Append(" (");
                stringBuilder.Append(InstanceId);
                stringBuilder.Append(")");
            }

            stringBuilder.AppendLine();

            foreach (var dependency in Dependencies.OrderBy(d => d.Type.Name))
            {
                dependency.ToString(stringBuilder, level + 1);
            }
        }

        public static TypeWithDependecies GetTypeWithDependecies(object o)
        {
            return GetTypeWithDependecies(o, new ObjectIdProvider());
        }

        public static TypeWithDependecies GetTypeWithDependecies(object o, ObjectIdProvider objectIdProvider)
        {
            var type = o.GetType();
            var dependecies = new List<TypeWithDependecies>();

            foreach (var field in type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var value = field.GetValue(o);
                if (value != null)
                {
                    var valueType = value.GetType();
                    if (!IsIgnoredType(valueType))
                    {
                        if (IsStopType(valueType))
                            dependecies.Add(new TypeWithDependecies(valueType, new TypeWithDependecies[0], objectIdProvider.GetId(value)));
                        else
                            dependecies.Add(GetTypeWithDependecies(value, objectIdProvider));
                    }
                }
            }

            return new TypeWithDependecies(type, dependecies, objectIdProvider.GetId(o));
        }

        private static bool IsStopType(Type type)
        {
            if (type == typeof(XmlSerializer))
                return true;

            if (type.Namespace + "." + type.Name == "System.Collections.Generic.GenericEqualityComparer`1")
                return true;

            if (type == WebResourceNameEqualityComparer)
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntityRelationDataAccess<,,,,>))
                return true;

            if (type.FullName.StartsWith("Castle.Proxies"))
                return true;

            if (type == typeof(vCardStandardWriter))
                return true;

            if (type == typeof(ContactEntityMapper))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntitySyncStateEnvironment<,,,,,,>))
                return true;

            return false;
        }

        private static bool IsIgnoredType(Type type)
        {
            if (type.Namespace + "." + type.Name == "System.Collections.Generic.GenericEqualityComparer`1")
                return false;

            if (type.FullName.StartsWith("System.ValueTuple`2"))
                return false;

            if (type.Assembly == Mscorlib)
                return true;

            return false;
        }

        private static readonly Type WebResourceNameEqualityComparer = WebResourceName.Comparer.GetType();
        private static readonly Assembly Mscorlib = typeof(int).Assembly;
    }
}