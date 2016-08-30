using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping.Model
{
    [DebuggerDisplay("Class {Uri}")]
    internal class ClassMapping : IQueryableClassMapping
    {
        private static readonly AbsoluteUriComparer UriComparer = new AbsoluteUriComparer();
        private readonly Uri _uri;

        public ClassMapping(Uri uri, bool isInherited)
        {
            _uri = uri;
            IsInherited = isInherited;
        }

        public Uri Uri
        {
            get
            {
                return _uri;
            }
        }

        public bool IsInherited { get; private set; }

        public IEnumerable<Uri> Uris
        {
            get
            {
                yield return _uri;
            }
        }

        public static bool operator ==(ClassMapping left, ClassMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClassMapping left, ClassMapping right)
        {
            return !Equals(left, right);
        }

        public bool IsMatch(IEnumerable<Uri> entityClasses)
        {
            return entityClasses.Contains(_uri, AbsoluteUriComparer.Default);
        }

        public void Accept(IMappingModelVisitor mappingModelVisitor)
        {
            mappingModelVisitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }

            return Equals((ClassMapping)obj);
        }

        public override int GetHashCode()
        {
            return _uri.GetHashCode();
        }

        protected bool Equals(ClassMapping other)
        {
            return UriComparer.Equals(_uri, other._uri);
        }
    }
}