
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient.Server;

namespace Utility.DataAccess
{
    public class Binding
    {
        private readonly string _name;

        private readonly SqlDbType? _type;

        private readonly object _value;

        private readonly IReadOnlyCollection<object[]>? _array;

        private readonly SqlMetaData[]? _meta;

        private readonly string? _table;

        private readonly ParameterDirection? _direction = ParameterDirection.Input;

        private readonly int? _size;

        public Binding(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public Binding(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            _name = name;
            _value = value;
            _direction = direction;
        }

        public Binding(string name, object value, SqlDbType Type, int? Size = null, ParameterDirection direction = ParameterDirection.Input)
        {
            _name = name;
            _value = value;
            _direction = direction;
            _type = Type;
            _size = Size;
        }

        public Binding(string name, string table, SqlMetaData[] meta, IReadOnlyCollection<object[]> array)
        {
            _name = name;
            _table = table;
            _meta = meta;
            _array = array;
        }

        public SqlMetaData[] Meta
        {
            get
            {
                return _meta;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Table
        {
            get
            {
                return _table;
            }
        }

        public SqlDbType? Type
        {
            get
            {
                return _type;
            }
        }

        public object Value
        {
            get
            {
                return _value;
            }
        }

        public IReadOnlyCollection<object[]> Values
        {
            get
            {
                return _array;
            }
        }

        //public ParameterDirection ParamDirection
        //{
        //    get
        //    {
        //        return _direction;
        //    }
        //}

        public int? Size
        {
            get
            {
                return _size;
            }
        }
        public static List<Binding> GetBindings<IConvertible>(string name, IConvertible value, List<Binding> bindings = null, ParameterDirection direction = ParameterDirection.Input)
        {
            if (bindings == null)
                bindings = new List<Binding>();

            bindings.Add(new Binding(name, value, direction));

            return bindings;
        }
    }

}
