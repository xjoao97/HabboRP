using System;
using System.Collections;
using System.Collections.Generic;

namespace Plus.HabboHotel.Catalog
{
    public class CatalogBundle
    {
        private int _id;
        private string _title;
        private string _image;
        private string _link;

        public CatalogBundle (int Id, string Title, string Image, string Link)
        {
            this._id = Id;
            this._title = Title;
            this._image = Image;
            this._link = Link;
        }

        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }

        public string Image
        {
            get { return this._image; }
            set { this._image = value; }
        }

        public string Link
        {
            get { return this._link; }
            set { this._link = value; }
        }
    }
}