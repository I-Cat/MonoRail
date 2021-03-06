﻿//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.Blade

    open System
    open System.IO
    open System.Collections.Generic
    
    [<AbstractClass>]
    type BaseBladePage() = 

        abstract member Initialize : unit -> unit 
        abstract member PreRenderPage : unit -> unit 
        abstract member RenderPage : unit -> unit 
        abstract member PostRenderPage : unit -> unit 
        abstract member Write : obj -> unit
        abstract member Write : TextWriter * obj -> unit
        abstract member WriteLiteral : string -> unit
        abstract member WriteLiteral : TextWriter * string -> unit

        default x.Initialize() = 
            ()
        default x.PreRenderPage() = 
            ()
        default x.PostRenderPage() = 
            ()

