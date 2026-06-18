using NetworkService.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class DistributedEnergyResource : ValidationBase
    {
        private int _id = 0;
        private string _name = "";
        private EnergyResourceType _type;
        private double _value = 0;
        private bool _isAlarm = false;

        #region Properties
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public EnergyResourceType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public bool IsAlarm
        {
            get => _isAlarm;
            set => SetProperty(ref _isAlarm, value);
        }
        #endregion

        public DistributedEnergyResource() { }
        public DistributedEnergyResource(int id, string name, EnergyResourceType type, double value)
        {
            Id = id;
            Name = name;
            Type = type;
            Value = value;
        }

        protected override void ValidateSelf()
        {
            if (string.IsNullOrEmpty(this._name))
            {
                this.ValidationErrors["Name"] = "Name is required";
            }
            if (this._type == null) 
            {
                this.ValidationErrors["Type"] = "Type is required";
            }
        }
    }
}
