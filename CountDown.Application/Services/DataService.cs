﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using BigEgg.Framework.Applications;
using CountDown.Application.Domain;
using CountDown.Application.Models;
using CountDown.Application.Properties;

namespace CountDown.Application.Services
{
    [Export(typeof(IDataService)), Export]
    internal class DataService : DataModel, IDataService
    {
        #region Members
        private readonly ObservableCollection<ICountDownItem> countDownItems;
        private readonly ObservableCollection<ICountDownItem> alartItems;
        private readonly ObservableCollection<string> branches;
        private readonly ObservableCollection<ICountDownItem> selectItems;
        private readonly NewCountDownModel newCountDownModel;
        private ICommand newCountDownItem;
        private ICommand deleteCountDownItem;
        #endregion

        [ImportingConstructor]
        public DataService()
        {
            this.countDownItems = new ObservableCollection<ICountDownItem>();
            this.alartItems = new ObservableCollection<ICountDownItem>();
            this.branches = new ObservableCollection<string>();
            this.selectItems = new ObservableCollection<ICountDownItem>();
            this.newCountDownModel = new NewCountDownModel
            {
                Days = 0,
                Hours = 0,
                Minutes = 0,
                NoticeBranch = string.Empty,
                Notice = string.Empty,
                BeforeAlartMinutes = Settings.Default.DefautBeforeAlartMinutes
            };

            AddWeakEventListener(this.selectItems, SelectItemsChanged);
            AddWeakEventListener(this.newCountDownModel, NewCountDownModelPropertyChanged);
            AddWeakEventListener(this.alartItems, AlartItemsChanged);
        }


        #region Properties
        public ObservableCollection<ICountDownItem> CountDownItems 
        { 
            get 
            {
                this.countDownItems.OrderBy(i => i.Time);
                this.countDownItems.ToArray();
                return this.countDownItems; 
            } 
        }

        public ObservableCollection<ICountDownItem> AlartItems
        {
            get
            {
                this.alartItems.OrderBy(i => i.Time);
                this.alartItems.ToArray();
                return this.alartItems;
            }
        }

        public ObservableCollection<ICountDownItem> SelectItems { get { return this.selectItems; } }

        public ObservableCollection<string> Branches { get { return this.branches; } }

        public NewCountDownModel NewCountDownModel { get { return this.newCountDownModel; } }

        public ICommand NewCountDownItem
        {
            get { return this.newCountDownItem; }
            set
            {
                if (this.newCountDownItem != value)
                {
                    this.newCountDownItem = value;
                    RaisePropertyChanged("NewCountDownItem");
                }
            }
        }

        public ICommand DeleteCountDownItem
        {
            get { return this.deleteCountDownItem; }
            set
            {
                if (this.deleteCountDownItem != value)
                {
                    this.deleteCountDownItem = value;
                    RaisePropertyChanged("DeleteCountDownItem");
                }
            }
        }
        #endregion


        #region Public Methods
        public void CleanExpiredItems()
        {
            DateTime expiredTime = DateTime.Now.AddMinutes(0 - Settings.Default.DefaultExpiredMinutes);
            IEnumerable<ICountDownItem> expiredItems = this.alartItems.Where(
                c => (c.Time < expiredTime) && (c.HasAlart == true));

            foreach (ICountDownItem item in expiredItems)
            {
                this.alartItems.Remove(item);
            }
        }

        public void CheckAlartItems()
        {
            IEnumerable<ICountDownItem> newAlartItems = this.CountDownItems.Where(
                i => i.AlartTime < DateTime.Now);

            foreach(ICountDownItem item in newAlartItems)
            {
                this.countDownItems.Remove(item);
                this.alartItems.Add(item);
            }
        }
        #endregion

        #region Private Methods
        private void SelectItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(ICountDownItem item in e.NewItems)
                {
                    if (!this.CountDownItems.Contains(item)) { throw new Exception("Select item must contain in the items list."); }
                }
            }
            RaisePropertyChanged("SelectItems");
        }

        private void NewCountDownModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("NewCountDownModel");
        }

        private void AlartItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                RaisePropertyChanged("AlartItems");
            }
        }
        #endregion
    }
}
