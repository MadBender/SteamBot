﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KeyBot.Tests.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class FreeKeyOfferChecker {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FreeKeyOfferChecker() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KeyBot.Tests.Resources.FreeKeyOfferChecker", typeof(FreeKeyOfferChecker).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///	&quot;response&quot;: {
        ///		&quot;offer&quot;: {
        ///			&quot;tradeofferid&quot;: &quot;340556971&quot;,
        ///			&quot;accountid_other&quot;: 182633581,
        ///			&quot;message&quot;: &quot;&quot;,
        ///			&quot;expiration_time&quot;: 1425676278,
        ///			&quot;trade_offer_state&quot;: 2,
        ///			&quot;items_to_give&quot;: [
        ///				{
        ///					&quot;appid&quot;: &quot;730&quot;,
        ///					&quot;contextid&quot;: &quot;2&quot;,
        ///					&quot;assetid&quot;: &quot;1643749961&quot;,
        ///					&quot;classid&quot;: &quot;186150629&quot;,
        ///					&quot;instanceid&quot;: &quot;143865972&quot;,
        ///					&quot;amount&quot;: &quot;1&quot;,
        ///					&quot;missing&quot;: false
        ///				}
        ///			],
        ///			&quot;items_to_receive&quot;: [
        ///				{
        ///					&quot;appid&quot;: &quot;730&quot;,
        ///					&quot;contextid&quot;: &quot;2&quot;,
        ///					&quot;assetid&quot;: &quot;157663 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CorrectOffer {
            get {
                return ResourceManager.GetString("CorrectOffer", resourceCulture);
            }
        }
    }
}
