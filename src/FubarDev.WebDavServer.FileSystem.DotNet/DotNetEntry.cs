﻿// <copyright file="DotNetEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// A .NET <see cref="System.IO"/> based implementation of a WebDAV entry (collection or document)
    /// </summary>
    public abstract class DotNetEntry : IEntry
    {
        private readonly DotNetDirectory _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The file system information</param>
        /// <param name="path">The root-relative path of this entry</param>
        protected DotNetEntry(DotNetFileSystem fileSystem, DotNetDirectory parent, FileSystemInfo info, Uri path)
        {
            _parent = parent;
            Info = info;
            DotNetFileSystem = fileSystem;
            Path = path;
        }

        /// <summary>
        /// Gets the file system information of this entry
        /// </summary>
        public FileSystemInfo Info { get; }

        /// <summary>
        /// Gets the file system this entry belongs to
        /// </summary>
        public DotNetFileSystem DotNetFileSystem { get; }

        /// <inheritdoc />
        public string Name => Info.Name;

        /// <inheritdoc />
        public IFileSystem FileSystem => DotNetFileSystem;

        /// <inheritdoc />
        public ICollection Parent => _parent;

        /// <inheritdoc />
        public Uri Path { get; }

        /// <inheritdoc />
        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        /// <inheritdoc />
        public DateTime CreationTimeUtc => Info.CreationTimeUtc;

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public Task SetLastWriteTimeUtcAsync(DateTime lastWriteTime, CancellationToken cancellationToken)
        {
            Info.LastWriteTimeUtc = lastWriteTime;
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task SetCreationTimeUtcAsync(DateTime creationTime, CancellationToken cancellationToken)
        {
            Info.CreationTimeUtc = creationTime;
            return Task.FromResult(0);
        }
    }
}
