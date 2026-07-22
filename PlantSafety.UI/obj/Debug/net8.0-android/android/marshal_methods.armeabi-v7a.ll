; ModuleID = 'marshal_methods.armeabi-v7a.ll'
source_filename = "marshal_methods.armeabi-v7a.ll"
target datalayout = "e-m:e-p:32:32-Fi8-i64:64-v128:64:128-a:0:32-n32-S64"
target triple = "armv7-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [344 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [682 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 68
	i32 10166715, ; 1: System.Net.NameResolution.dll => 0x9b21bb => 67
	i32 15721112, ; 2: System.Runtime.Intrinsics.dll => 0xefe298 => 108
	i32 32687329, ; 3: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 264
	i32 34715100, ; 4: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 298
	i32 34839235, ; 5: System.IO.FileSystem.DriveInfo => 0x2139ac3 => 48
	i32 39485524, ; 6: System.Net.WebSockets.dll => 0x25a8054 => 80
	i32 42639949, ; 7: System.Threading.Thread => 0x28aa24d => 145
	i32 66541672, ; 8: System.Diagnostics.StackTrace => 0x3f75868 => 30
	i32 67008169, ; 9: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 339
	i32 68219467, ; 10: System.Security.Cryptography.Primitives => 0x410f24b => 124
	i32 72070932, ; 11: Microsoft.Maui.Graphics.dll => 0x44bb714 => 221
	i32 82292897, ; 12: System.Runtime.CompilerServices.VisualC.dll => 0x4e7b0a1 => 102
	i32 98325684, ; 13: Microsoft.Extensions.Diagnostics.Abstractions => 0x5dc54b4 => 198
	i32 101534019, ; 14: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 282
	i32 117431740, ; 15: System.Runtime.InteropServices => 0x6ffddbc => 107
	i32 120558881, ; 16: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 282
	i32 122350210, ; 17: System.Threading.Channels.dll => 0x74aea82 => 139
	i32 134690465, ; 18: Xamarin.Kotlin.StdLib.Jdk7.dll => 0x80736a1 => 302
	i32 142721839, ; 19: System.Net.WebHeaderCollection => 0x881c32f => 77
	i32 149972175, ; 20: System.Security.Cryptography.Primitives.dll => 0x8f064cf => 124
	i32 159306688, ; 21: System.ComponentModel.Annotations => 0x97ed3c0 => 13
	i32 165246403, ; 22: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 238
	i32 176265551, ; 23: System.ServiceProcess => 0xa81994f => 132
	i32 182336117, ; 24: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 284
	i32 184328833, ; 25: System.ValueTuple.dll => 0xafca281 => 151
	i32 195452805, ; 26: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 336
	i32 199333315, ; 27: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 337
	i32 205061960, ; 28: System.ComponentModel => 0xc38ff48 => 18
	i32 209399409, ; 29: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 236
	i32 220171995, ; 30: System.Diagnostics.Debug => 0xd1f8edb => 26
	i32 221063263, ; 31: Microsoft.AspNetCore.Http.Connections.Client => 0xd2d285f => 183
	i32 221958352, ; 32: Microsoft.Extensions.Diagnostics.dll => 0xd3ad0d0 => 197
	i32 230216969, ; 33: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 258
	i32 230752869, ; 34: Microsoft.CSharp.dll => 0xdc10265 => 1
	i32 231409092, ; 35: System.Linq.Parallel => 0xdcb05c4 => 59
	i32 231814094, ; 36: System.Globalization => 0xdd133ce => 42
	i32 246610117, ; 37: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 91
	i32 254259026, ; 38: Microsoft.AspNetCore.Components.dll => 0xf27af52 => 177
	i32 261689757, ; 39: Xamarin.AndroidX.ConstraintLayout.dll => 0xf99119d => 241
	i32 276479776, ; 40: System.Threading.Timer.dll => 0x107abf20 => 147
	i32 278686392, ; 41: Xamarin.AndroidX.Lifecycle.LiveData.dll => 0x109c6ab8 => 260
	i32 280482487, ; 42: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 257
	i32 280992041, ; 43: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 308
	i32 291076382, ; 44: System.IO.Pipes.AccessControl.dll => 0x1159791e => 54
	i32 291275502, ; 45: Microsoft.Extensions.Http.dll => 0x115c82ee => 205
	i32 298918909, ; 46: System.Net.Ping.dll => 0x11d123fd => 69
	i32 317674968, ; 47: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 336
	i32 318968648, ; 48: Xamarin.AndroidX.Activity.dll => 0x13031348 => 227
	i32 321597661, ; 49: System.Numerics => 0x132b30dd => 83
	i32 336156722, ; 50: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 321
	i32 342366114, ; 51: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 259
	i32 348048101, ; 52: Microsoft.AspNetCore.Http.Connections.Common.dll => 0x14becae5 => 184
	i32 356389973, ; 53: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 320
	i32 360082299, ; 54: System.ServiceModel.Web => 0x15766b7b => 131
	i32 367780167, ; 55: System.IO.Pipes => 0x15ebe147 => 55
	i32 374914964, ; 56: System.Transactions.Local => 0x1658bf94 => 149
	i32 375677976, ; 57: System.Net.ServicePoint.dll => 0x16646418 => 74
	i32 379916513, ; 58: System.Threading.Thread.dll => 0x16a510e1 => 145
	i32 385762202, ; 59: System.Memory.dll => 0x16fe439a => 62
	i32 392610295, ; 60: System.Threading.ThreadPool.dll => 0x1766c1f7 => 146
	i32 395744057, ; 61: _Microsoft.Android.Resource.Designer => 0x17969339 => 340
	i32 403441872, ; 62: WindowsBase => 0x180c08d0 => 165
	i32 435591531, ; 63: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 332
	i32 441335492, ; 64: Xamarin.AndroidX.ConstraintLayout.Core => 0x1a4e3ec4 => 242
	i32 442565967, ; 65: System.Collections => 0x1a61054f => 12
	i32 450948140, ; 66: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 255
	i32 451504562, ; 67: System.Security.Cryptography.X509Certificates => 0x1ae969b2 => 125
	i32 456227837, ; 68: System.Web.HttpUtility.dll => 0x1b317bfd => 152
	i32 458494020, ; 69: Microsoft.AspNetCore.SignalR.Common.dll => 0x1b541044 => 188
	i32 459347974, ; 70: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 113
	i32 465846621, ; 71: mscorlib => 0x1bc4415d => 166
	i32 469710990, ; 72: System.dll => 0x1bff388e => 164
	i32 476646585, ; 73: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 257
	i32 485463106, ; 74: Microsoft.IdentityModel.Abstractions => 0x1cef9442 => 214
	i32 486930444, ; 75: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 270
	i32 498788369, ; 76: System.ObjectModel => 0x1dbae811 => 84
	i32 500358224, ; 77: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 319
	i32 503918385, ; 78: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 313
	i32 513247710, ; 79: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 211
	i32 526420162, ; 80: System.Transactions.dll => 0x1f6088c2 => 150
	i32 527452488, ; 81: Xamarin.Kotlin.StdLib.Jdk7 => 0x1f704948 => 302
	i32 530272170, ; 82: System.Linq.Queryable => 0x1f9b4faa => 60
	i32 539058512, ; 83: Microsoft.Extensions.Logging => 0x20216150 => 206
	i32 540030774, ; 84: System.IO.FileSystem.dll => 0x20303736 => 51
	i32 545304856, ; 85: System.Runtime.Extensions => 0x2080b118 => 103
	i32 546455878, ; 86: System.Runtime.Serialization.Xml => 0x20924146 => 114
	i32 549171840, ; 87: System.Globalization.Calendars => 0x20bbb280 => 40
	i32 557405415, ; 88: Jsr305Binding => 0x213954e7 => 295
	i32 569601784, ; 89: Xamarin.AndroidX.Window.Extensions.Core.Core => 0x21f36ef8 => 293
	i32 571435654, ; 90: Microsoft.Extensions.FileProviders.Embedded.dll => 0x220f6a86 => 202
	i32 577335427, ; 91: System.Security.Cryptography.Cng => 0x22697083 => 120
	i32 592146354, ; 92: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 327
	i32 596856236, ; 93: PlantSafety.UI => 0x23934dac => 0
	i32 601371474, ; 94: System.IO.IsolatedStorage.dll => 0x23d83352 => 52
	i32 605376203, ; 95: System.IO.Compression.FileSystem => 0x24154ecb => 44
	i32 613668793, ; 96: System.Security.Cryptography.Algorithms => 0x2493d7b9 => 119
	i32 627609679, ; 97: Xamarin.AndroidX.CustomView => 0x2568904f => 247
	i32 627931235, ; 98: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 325
	i32 639843206, ; 99: Xamarin.AndroidX.Emoji2.ViewsHelper.dll => 0x26233b86 => 253
	i32 643868501, ; 100: System.Net => 0x2660a755 => 81
	i32 662205335, ; 101: System.Text.Encodings.Web.dll => 0x27787397 => 136
	i32 663517072, ; 102: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 289
	i32 666292255, ; 103: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 234
	i32 672442732, ; 104: System.Collections.Concurrent => 0x2814a96c => 8
	i32 683518922, ; 105: System.Net.Security => 0x28bdabca => 73
	i32 688181140, ; 106: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 307
	i32 690569205, ; 107: System.Xml.Linq.dll => 0x29293ff5 => 155
	i32 691348768, ; 108: Xamarin.KotlinX.Coroutines.Android.dll => 0x29352520 => 304
	i32 693804605, ; 109: System.Windows => 0x295a9e3d => 154
	i32 699345723, ; 110: System.Reflection.Emit => 0x29af2b3b => 92
	i32 700284507, ; 111: Xamarin.Jetbrains.Annotations => 0x29bd7e5b => 299
	i32 700358131, ; 112: System.IO.Compression.ZipFile => 0x29be9df3 => 45
	i32 706645707, ; 113: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 322
	i32 709557578, ; 114: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 310
	i32 720511267, ; 115: Xamarin.Kotlin.StdLib.Jdk8 => 0x2af22123 => 303
	i32 722857257, ; 116: System.Runtime.Loader.dll => 0x2b15ed29 => 109
	i32 731701662, ; 117: Microsoft.Extensions.Options.ConfigurationExtensions => 0x2b9ce19e => 210
	i32 735137430, ; 118: System.Security.SecureString.dll => 0x2bd14e96 => 129
	i32 752232764, ; 119: System.Diagnostics.Contracts.dll => 0x2cd6293c => 25
	i32 755313932, ; 120: Xamarin.Android.Glide.Annotations.dll => 0x2d052d0c => 224
	i32 759454413, ; 121: System.Net.Requests => 0x2d445acd => 72
	i32 762598435, ; 122: System.IO.Pipes.dll => 0x2d745423 => 55
	i32 775507847, ; 123: System.IO.Compression => 0x2e394f87 => 46
	i32 777317022, ; 124: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 331
	i32 789151979, ; 125: Microsoft.Extensions.Options => 0x2f0980eb => 209
	i32 790371945, ; 126: Xamarin.AndroidX.CustomView.PoolingContainer.dll => 0x2f1c1e69 => 248
	i32 804008546, ; 127: Microsoft.AspNetCore.Components.WebView.Maui => 0x2fec3262 => 181
	i32 804715423, ; 128: System.Data.Common => 0x2ff6fb9f => 22
	i32 807930345, ; 129: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx.dll => 0x302809e9 => 262
	i32 823281589, ; 130: System.Private.Uri.dll => 0x311247b5 => 86
	i32 830298997, ; 131: System.IO.Compression.Brotli => 0x317d5b75 => 43
	i32 832635846, ; 132: System.Xml.XPath.dll => 0x31a103c6 => 160
	i32 832711436, ; 133: Microsoft.AspNetCore.SignalR.Protocols.Json.dll => 0x31a22b0c => 189
	i32 834051424, ; 134: System.Net.Quic => 0x31b69d60 => 71
	i32 843511501, ; 135: Xamarin.AndroidX.Print => 0x3246f6cd => 275
	i32 873119928, ; 136: Microsoft.VisualBasic => 0x340ac0b8 => 3
	i32 877678880, ; 137: System.Globalization.dll => 0x34505120 => 42
	i32 878954865, ; 138: System.Net.Http.Json => 0x3463c971 => 63
	i32 904024072, ; 139: System.ComponentModel.Primitives.dll => 0x35e25008 => 16
	i32 911108515, ; 140: System.IO.MemoryMappedFiles.dll => 0x364e69a3 => 53
	i32 926902833, ; 141: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 334
	i32 928116545, ; 142: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 298
	i32 940202475, ; 143: Mapster => 0x380a59eb => 174
	i32 952186615, ; 144: System.Runtime.InteropServices.JavaScript.dll => 0x38c136f7 => 105
	i32 956575887, ; 145: Xamarin.Kotlin.StdLib.Jdk8.dll => 0x3904308f => 303
	i32 966729478, ; 146: Xamarin.Google.Crypto.Tink.Android => 0x399f1f06 => 296
	i32 967690846, ; 147: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 259
	i32 975236339, ; 148: System.Diagnostics.Tracing => 0x3a20ecf3 => 34
	i32 975874589, ; 149: System.Xml.XDocument => 0x3a2aaa1d => 158
	i32 986514023, ; 150: System.Private.DataContractSerialization.dll => 0x3acd0267 => 85
	i32 987214855, ; 151: System.Diagnostics.Tools => 0x3ad7b407 => 32
	i32 992768348, ; 152: System.Collections.dll => 0x3b2c715c => 12
	i32 994442037, ; 153: System.IO.FileSystem => 0x3b45fb35 => 51
	i32 999186168, ; 154: Microsoft.Extensions.FileSystemGlobbing.dll => 0x3b8e5ef8 => 204
	i32 1001831731, ; 155: System.IO.UnmanagedMemoryStream.dll => 0x3bb6bd33 => 56
	i32 1012816738, ; 156: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 279
	i32 1019214401, ; 157: System.Drawing => 0x3cbffa41 => 36
	i32 1028951442, ; 158: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 196
	i32 1029334545, ; 159: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 309
	i32 1031528504, ; 160: Xamarin.Google.ErrorProne.Annotations.dll => 0x3d7be038 => 297
	i32 1035644815, ; 161: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 232
	i32 1036536393, ; 162: System.Drawing.Primitives.dll => 0x3dc84a49 => 35
	i32 1040019210, ; 163: Mapster.Core.dll => 0x3dfd6f0a => 175
	i32 1044663988, ; 164: System.Linq.Expressions.dll => 0x3e444eb4 => 58
	i32 1048992957, ; 165: Microsoft.Extensions.Diagnostics.Abstractions.dll => 0x3e865cbd => 198
	i32 1052210849, ; 166: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 266
	i32 1058641855, ; 167: Microsoft.AspNetCore.Http.Connections.Common => 0x3f1997bf => 184
	i32 1062017875, ; 168: Microsoft.Identity.Client.Extensions.Msal => 0x3f4d1b53 => 213
	i32 1067306892, ; 169: GoogleGson => 0x3f9dcf8c => 173
	i32 1082857460, ; 170: System.ComponentModel.TypeConverter => 0x408b17f4 => 17
	i32 1084122840, ; 171: Xamarin.Kotlin.StdLib => 0x409e66d8 => 300
	i32 1098259244, ; 172: System => 0x41761b2c => 164
	i32 1106973742, ; 173: Microsoft.Extensions.Configuration.FileExtensions.dll => 0x41fb142e => 193
	i32 1118262833, ; 174: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 322
	i32 1121599056, ; 175: Xamarin.AndroidX.Lifecycle.Runtime.Ktx.dll => 0x42da3e50 => 265
	i32 1127624469, ; 176: Microsoft.Extensions.Logging.Debug => 0x43362f15 => 208
	i32 1149092582, ; 177: Xamarin.AndroidX.Window => 0x447dc2e6 => 292
	i32 1168523401, ; 178: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 328
	i32 1170634674, ; 179: System.Web.dll => 0x45c677b2 => 153
	i32 1173126369, ; 180: Microsoft.Extensions.FileProviders.Abstractions.dll => 0x45ec7ce1 => 200
	i32 1175144683, ; 181: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 288
	i32 1178241025, ; 182: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 273
	i32 1203215381, ; 183: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 326
	i32 1204270330, ; 184: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 234
	i32 1208641965, ; 185: System.Diagnostics.Process => 0x480a69ad => 29
	i32 1219128291, ; 186: System.IO.IsolatedStorage => 0x48aa6be3 => 52
	i32 1233093933, ; 187: Microsoft.AspNetCore.SignalR.Client.Core.dll => 0x497f852d => 187
	i32 1234928153, ; 188: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 324
	i32 1243150071, ; 189: Xamarin.AndroidX.Window.Extensions.Core.Core.dll => 0x4a18f6f7 => 293
	i32 1253011324, ; 190: Microsoft.Win32.Registry => 0x4aaf6f7c => 5
	i32 1260983243, ; 191: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 308
	i32 1264511973, ; 192: Xamarin.AndroidX.Startup.StartupRuntime.dll => 0x4b5eebe5 => 283
	i32 1267360935, ; 193: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 287
	i32 1273260888, ; 194: Xamarin.AndroidX.Collection.Ktx => 0x4be46b58 => 239
	i32 1275534314, ; 195: Xamarin.KotlinX.Coroutines.Android => 0x4c071bea => 304
	i32 1278448581, ; 196: Xamarin.AndroidX.Annotation.Jvm => 0x4c3393c5 => 231
	i32 1293217323, ; 197: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 250
	i32 1309188875, ; 198: System.Private.DataContractSerialization => 0x4e08a30b => 85
	i32 1322716291, ; 199: Xamarin.AndroidX.Window.dll => 0x4ed70c83 => 292
	i32 1324164729, ; 200: System.Linq => 0x4eed2679 => 61
	i32 1335329327, ; 201: System.Runtime.Serialization.Json.dll => 0x4f97822f => 112
	i32 1364015309, ; 202: System.IO => 0x514d38cd => 57
	i32 1373134921, ; 203: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 338
	i32 1376866003, ; 204: Xamarin.AndroidX.SavedState => 0x52114ed3 => 279
	i32 1379779777, ; 205: System.Resources.ResourceManager => 0x523dc4c1 => 99
	i32 1402170036, ; 206: System.Configuration.dll => 0x53936ab4 => 19
	i32 1406073936, ; 207: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 243
	i32 1408764838, ; 208: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 111
	i32 1411638395, ; 209: System.Runtime.CompilerServices.Unsafe => 0x5423e47b => 101
	i32 1414043276, ; 210: Microsoft.AspNetCore.Connections.Abstractions.dll => 0x5448968c => 182
	i32 1422545099, ; 211: System.Runtime.CompilerServices.VisualC => 0x54ca50cb => 102
	i32 1430672901, ; 212: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 306
	i32 1434145427, ; 213: System.Runtime.Handles => 0x557b5293 => 104
	i32 1435222561, ; 214: Xamarin.Google.Crypto.Tink.Android.dll => 0x558bc221 => 296
	i32 1439761251, ; 215: System.Net.Quic.dll => 0x55d10363 => 71
	i32 1452070440, ; 216: System.Formats.Asn1.dll => 0x568cd628 => 38
	i32 1453312822, ; 217: System.Diagnostics.Tools.dll => 0x569fcb36 => 32
	i32 1454105418, ; 218: Microsoft.Extensions.FileProviders.Composite => 0x56abe34a => 201
	i32 1457743152, ; 219: System.Runtime.Extensions.dll => 0x56e36530 => 103
	i32 1458022317, ; 220: System.Net.Security.dll => 0x56e7a7ad => 73
	i32 1461004990, ; 221: es\Microsoft.Maui.Controls.resources => 0x57152abe => 312
	i32 1461234159, ; 222: System.Collections.Immutable.dll => 0x5718a9ef => 9
	i32 1461719063, ; 223: System.Security.Cryptography.OpenSsl => 0x57201017 => 123
	i32 1462112819, ; 224: System.IO.Compression.dll => 0x57261233 => 46
	i32 1469204771, ; 225: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 233
	i32 1470490898, ; 226: Microsoft.Extensions.Primitives => 0x57a5e912 => 211
	i32 1479771757, ; 227: System.Collections.Immutable => 0x5833866d => 9
	i32 1480492111, ; 228: System.IO.Compression.Brotli.dll => 0x583e844f => 43
	i32 1487239319, ; 229: Microsoft.Win32.Primitives => 0x58a57897 => 4
	i32 1490025113, ; 230: Xamarin.AndroidX.SavedState.SavedState.Ktx.dll => 0x58cffa99 => 280
	i32 1493001747, ; 231: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 316
	i32 1505131794, ; 232: Microsoft.Extensions.Http => 0x59b67d12 => 205
	i32 1514721132, ; 233: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 311
	i32 1521091094, ; 234: Microsoft.Extensions.FileSystemGlobbing => 0x5aaa0216 => 204
	i32 1536373174, ; 235: System.Diagnostics.TextWriterTraceListener => 0x5b9331b6 => 31
	i32 1543031311, ; 236: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 138
	i32 1543355203, ; 237: System.Reflection.Emit.dll => 0x5bfdbb43 => 92
	i32 1546581739, ; 238: Microsoft.AspNetCore.Components.WebView.Maui.dll => 0x5c2ef6eb => 181
	i32 1550322496, ; 239: System.Reflection.Extensions.dll => 0x5c680b40 => 93
	i32 1551623176, ; 240: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 331
	i32 1565862583, ; 241: System.IO.FileSystem.Primitives => 0x5d552ab7 => 49
	i32 1566207040, ; 242: System.Threading.Tasks.Dataflow.dll => 0x5d5a6c40 => 141
	i32 1573704789, ; 243: System.Runtime.Serialization.Json => 0x5dccd455 => 112
	i32 1580037396, ; 244: System.Threading.Overlapped => 0x5e2d7514 => 140
	i32 1582372066, ; 245: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 249
	i32 1592978981, ; 246: System.Runtime.Serialization.dll => 0x5ef2ee25 => 115
	i32 1597949149, ; 247: Xamarin.Google.ErrorProne.Annotations => 0x5f3ec4dd => 297
	i32 1601112923, ; 248: System.Xml.Serialization => 0x5f6f0b5b => 157
	i32 1604827217, ; 249: System.Net.WebClient => 0x5fa7b851 => 76
	i32 1618516317, ; 250: System.Net.WebSockets.Client.dll => 0x6078995d => 79
	i32 1622152042, ; 251: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 269
	i32 1622358360, ; 252: System.Dynamic.Runtime => 0x60b33958 => 37
	i32 1624863272, ; 253: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 291
	i32 1632842087, ; 254: Microsoft.Extensions.Configuration.Json => 0x61533167 => 194
	i32 1635184631, ; 255: Xamarin.AndroidX.Emoji2.ViewsHelper => 0x6176eff7 => 253
	i32 1636350590, ; 256: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 246
	i32 1639515021, ; 257: System.Net.Http.dll => 0x61b9038d => 64
	i32 1639986890, ; 258: System.Text.RegularExpressions => 0x61c036ca => 138
	i32 1641389582, ; 259: System.ComponentModel.EventBasedAsync.dll => 0x61d59e0e => 15
	i32 1654881142, ; 260: Microsoft.AspNetCore.Components.WebView => 0x62a37b76 => 180
	i32 1657153582, ; 261: System.Runtime => 0x62c6282e => 116
	i32 1658241508, ; 262: Xamarin.AndroidX.Tracing.Tracing.dll => 0x62d6c1e4 => 285
	i32 1658251792, ; 263: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 294
	i32 1670060433, ; 264: Xamarin.AndroidX.ConstraintLayout => 0x638b1991 => 241
	i32 1675553242, ; 265: System.IO.FileSystem.DriveInfo.dll => 0x63dee9da => 48
	i32 1677501392, ; 266: System.Net.Primitives.dll => 0x63fca3d0 => 70
	i32 1678508291, ; 267: System.Net.WebSockets => 0x640c0103 => 80
	i32 1679769178, ; 268: System.Security.Cryptography => 0x641f3e5a => 126
	i32 1691477237, ; 269: System.Reflection.Metadata => 0x64d1e4f5 => 94
	i32 1696967625, ; 270: System.Security.Cryptography.Csp => 0x6525abc9 => 121
	i32 1698840827, ; 271: Xamarin.Kotlin.StdLib.Common => 0x654240fb => 301
	i32 1701541528, ; 272: System.Diagnostics.Debug.dll => 0x656b7698 => 26
	i32 1720223769, ; 273: Xamarin.AndroidX.Lifecycle.LiveData.Core.Ktx => 0x66888819 => 262
	i32 1726116996, ; 274: System.Reflection.dll => 0x66e27484 => 97
	i32 1728033016, ; 275: System.Diagnostics.FileVersionInfo.dll => 0x66ffb0f8 => 28
	i32 1729485958, ; 276: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 237
	i32 1736233607, ; 277: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 329
	i32 1743415430, ; 278: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 307
	i32 1744735666, ; 279: System.Transactions.Local.dll => 0x67fe8db2 => 149
	i32 1746115085, ; 280: System.IO.Pipelines.dll => 0x68139a0d => 222
	i32 1746316138, ; 281: Mono.Android.Export => 0x6816ab6a => 169
	i32 1750313021, ; 282: Microsoft.Win32.Primitives.dll => 0x6853a83d => 4
	i32 1758240030, ; 283: System.Resources.Reader.dll => 0x68cc9d1e => 98
	i32 1760259689, ; 284: Microsoft.AspNetCore.Components.Web.dll => 0x68eb6e69 => 179
	i32 1763938596, ; 285: System.Diagnostics.TraceSource.dll => 0x69239124 => 33
	i32 1765942094, ; 286: System.Reflection.Extensions => 0x6942234e => 93
	i32 1766324549, ; 287: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 284
	i32 1770582343, ; 288: Microsoft.Extensions.Logging.dll => 0x6988f147 => 206
	i32 1776026572, ; 289: System.Core.dll => 0x69dc03cc => 21
	i32 1777075843, ; 290: System.Globalization.Extensions.dll => 0x69ec0683 => 41
	i32 1780572499, ; 291: Mono.Android.Runtime.dll => 0x6a216153 => 170
	i32 1782862114, ; 292: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 323
	i32 1788241197, ; 293: Xamarin.AndroidX.Fragment => 0x6a96652d => 255
	i32 1793755602, ; 294: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 315
	i32 1794500907, ; 295: Microsoft.Identity.Client.dll => 0x6af5e92b => 212
	i32 1795738900, ; 296: Mapster.Core => 0x6b08cd14 => 175
	i32 1808609942, ; 297: Xamarin.AndroidX.Loader => 0x6bcd3296 => 269
	i32 1813058853, ; 298: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 300
	i32 1813201214, ; 299: Xamarin.Google.Android.Material => 0x6c13413e => 294
	i32 1818569960, ; 300: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 274
	i32 1818787751, ; 301: Microsoft.VisualBasic.Core => 0x6c687fa7 => 2
	i32 1824175904, ; 302: System.Text.Encoding.Extensions => 0x6cbab720 => 134
	i32 1824722060, ; 303: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 111
	i32 1828688058, ; 304: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 207
	i32 1842015223, ; 305: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 335
	i32 1847515442, ; 306: Xamarin.Android.Glide.Annotations => 0x6e1ed932 => 224
	i32 1853025655, ; 307: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 332
	i32 1858542181, ; 308: System.Linq.Expressions => 0x6ec71a65 => 58
	i32 1870277092, ; 309: System.Reflection.Primitives => 0x6f7a29e4 => 95
	i32 1875935024, ; 310: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 314
	i32 1879696579, ; 311: System.Formats.Tar.dll => 0x7009e4c3 => 39
	i32 1885316902, ; 312: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 235
	i32 1888955245, ; 313: System.Diagnostics.Contracts => 0x70972b6d => 25
	i32 1889954781, ; 314: System.Reflection.Metadata.dll => 0x70a66bdd => 94
	i32 1898237753, ; 315: System.Reflection.DispatchProxy => 0x7124cf39 => 89
	i32 1900610850, ; 316: System.Resources.ResourceManager.dll => 0x71490522 => 99
	i32 1910275211, ; 317: System.Collections.NonGeneric.dll => 0x71dc7c8b => 10
	i32 1939592360, ; 318: System.Private.Xml.Linq => 0x739bd4a8 => 87
	i32 1945717188, ; 319: Microsoft.AspNetCore.SignalR.Client.Core => 0x73f949c4 => 187
	i32 1956758971, ; 320: System.Resources.Writer => 0x74a1c5bb => 100
	i32 1961813231, ; 321: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x74eee4ef => 281
	i32 1967334205, ; 322: Microsoft.AspNetCore.SignalR.Common => 0x7543233d => 188
	i32 1968388702, ; 323: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 190
	i32 1983156543, ; 324: Xamarin.Kotlin.StdLib.Common.dll => 0x7634913f => 301
	i32 1985761444, ; 325: Xamarin.Android.Glide.GifDecoder => 0x765c50a4 => 226
	i32 2003115576, ; 326: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 311
	i32 2011961780, ; 327: System.Buffers.dll => 0x77ec19b4 => 7
	i32 2019465201, ; 328: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 266
	i32 2025202353, ; 329: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 306
	i32 2031763787, ; 330: Xamarin.Android.Glide => 0x791a414b => 223
	i32 2040764568, ; 331: Microsoft.Identity.Client.Extensions.Msal.dll => 0x79a39898 => 213
	i32 2045470958, ; 332: System.Private.Xml => 0x79eb68ee => 88
	i32 2048278909, ; 333: Microsoft.Extensions.Configuration.Binder.dll => 0x7a16417d => 192
	i32 2055257422, ; 334: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 261
	i32 2060060697, ; 335: System.Windows.dll => 0x7aca0819 => 154
	i32 2066184531, ; 336: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 310
	i32 2070888862, ; 337: System.Diagnostics.TraceSource => 0x7b6f419e => 33
	i32 2072397586, ; 338: Microsoft.Extensions.FileProviders.Physical => 0x7b864712 => 203
	i32 2079903147, ; 339: System.Runtime.dll => 0x7bf8cdab => 116
	i32 2090596640, ; 340: System.Numerics.Vectors => 0x7c9bf920 => 82
	i32 2127167465, ; 341: System.Console => 0x7ec9ffe9 => 20
	i32 2142473426, ; 342: System.Collections.Specialized => 0x7fb38cd2 => 11
	i32 2143790110, ; 343: System.Xml.XmlSerializer.dll => 0x7fc7a41e => 162
	i32 2146852085, ; 344: Microsoft.VisualBasic.dll => 0x7ff65cf5 => 3
	i32 2159891885, ; 345: Microsoft.Maui => 0x80bd55ad => 219
	i32 2169148018, ; 346: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 318
	i32 2181898931, ; 347: Microsoft.Extensions.Options.dll => 0x820d22b3 => 209
	i32 2192057212, ; 348: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 207
	i32 2193016926, ; 349: System.ObjectModel.dll => 0x82b6c85e => 84
	i32 2201107256, ; 350: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 305
	i32 2201231467, ; 351: System.Net.Http => 0x8334206b => 64
	i32 2207618523, ; 352: it\Microsoft.Maui.Controls.resources => 0x839595db => 320
	i32 2217644978, ; 353: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 288
	i32 2222056684, ; 354: System.Threading.Tasks.Parallel => 0x8471e4ec => 143
	i32 2229158877, ; 355: Microsoft.Extensions.Features.dll => 0x84de43dd => 199
	i32 2244775296, ; 356: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 270
	i32 2252106437, ; 357: System.Xml.Serialization.dll => 0x863c6ac5 => 157
	i32 2256313426, ; 358: System.Globalization.Extensions => 0x867c9c52 => 41
	i32 2265110946, ; 359: System.Security.AccessControl.dll => 0x8702d9a2 => 117
	i32 2266799131, ; 360: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 191
	i32 2267999099, ; 361: Xamarin.Android.Glide.DiskLruCache.dll => 0x872eeb7b => 225
	i32 2270573516, ; 362: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 314
	i32 2279755925, ; 363: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 277
	i32 2293034957, ; 364: System.ServiceModel.Web.dll => 0x88acefcd => 131
	i32 2295906218, ; 365: System.Net.Sockets => 0x88d8bfaa => 75
	i32 2298471582, ; 366: System.Net.Mail => 0x88ffe49e => 66
	i32 2303942373, ; 367: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 324
	i32 2305521784, ; 368: System.Private.CoreLib.dll => 0x896b7878 => 172
	i32 2315684594, ; 369: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 229
	i32 2319144366, ; 370: Microsoft.AspNetCore.SignalR.Client => 0x8a3b55ae => 186
	i32 2320631194, ; 371: System.Threading.Tasks.Parallel.dll => 0x8a52059a => 143
	i32 2340441535, ; 372: System.Runtime.InteropServices.RuntimeInformation.dll => 0x8b804dbf => 106
	i32 2344264397, ; 373: System.ValueTuple => 0x8bbaa2cd => 151
	i32 2353062107, ; 374: System.Net.Primitives => 0x8c40e0db => 70
	i32 2368005991, ; 375: System.Xml.ReaderWriter.dll => 0x8d24e767 => 156
	i32 2371007202, ; 376: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 190
	i32 2378619854, ; 377: System.Security.Cryptography.Csp.dll => 0x8dc6dbce => 121
	i32 2383496789, ; 378: System.Security.Principal.Windows.dll => 0x8e114655 => 127
	i32 2395872292, ; 379: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 319
	i32 2401565422, ; 380: System.Web.HttpUtility => 0x8f24faee => 152
	i32 2403452196, ; 381: Xamarin.AndroidX.Emoji2.dll => 0x8f41c524 => 252
	i32 2411328690, ; 382: Microsoft.AspNetCore.Components => 0x8fb9f4b2 => 177
	i32 2421380589, ; 383: System.Threading.Tasks.Dataflow => 0x905355ed => 141
	i32 2423080555, ; 384: Xamarin.AndroidX.Collection.Ktx.dll => 0x906d466b => 239
	i32 2427813419, ; 385: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 316
	i32 2435356389, ; 386: System.Console.dll => 0x912896e5 => 20
	i32 2435904999, ; 387: System.ComponentModel.DataAnnotations.dll => 0x9130f5e7 => 14
	i32 2442556106, ; 388: Microsoft.JSInterop.dll => 0x919672ca => 215
	i32 2454642406, ; 389: System.Text.Encoding.dll => 0x924edee6 => 135
	i32 2458678730, ; 390: System.Net.Sockets.dll => 0x928c75ca => 75
	i32 2459001652, ; 391: System.Linq.Parallel.dll => 0x92916334 => 59
	i32 2465532216, ; 392: Xamarin.AndroidX.ConstraintLayout.Core.dll => 0x92f50938 => 242
	i32 2471841756, ; 393: netstandard.dll => 0x93554fdc => 167
	i32 2475788418, ; 394: Java.Interop.dll => 0x93918882 => 168
	i32 2480646305, ; 395: Microsoft.Maui.Controls => 0x93dba8a1 => 217
	i32 2483903535, ; 396: System.ComponentModel.EventBasedAsync => 0x940d5c2f => 15
	i32 2484371297, ; 397: System.Net.ServicePoint => 0x94147f61 => 74
	i32 2490993605, ; 398: System.AppContext.dll => 0x94798bc5 => 6
	i32 2501346920, ; 399: System.Data.DataSetExtensions => 0x95178668 => 23
	i32 2505896520, ; 400: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 264
	i32 2522472828, ; 401: Xamarin.Android.Glide.dll => 0x9659e17c => 223
	i32 2527013235, ; 402: PlantSafety.UI.dll => 0x969f2973 => 0
	i32 2537015816, ; 403: Microsoft.AspNetCore.Authorization => 0x9737ca08 => 176
	i32 2538310050, ; 404: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 91
	i32 2550873716, ; 405: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 317
	i32 2562349572, ; 406: Microsoft.CSharp => 0x98ba5a04 => 1
	i32 2570120770, ; 407: System.Text.Encodings.Web => 0x9930ee42 => 136
	i32 2581783588, ; 408: Xamarin.AndroidX.Lifecycle.Runtime.Ktx => 0x99e2e424 => 265
	i32 2581819634, ; 409: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 287
	i32 2585220780, ; 410: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 134
	i32 2585805581, ; 411: System.Net.Ping => 0x9a20430d => 69
	i32 2585813321, ; 412: Microsoft.AspNetCore.Components.Forms => 0x9a206149 => 178
	i32 2589602615, ; 413: System.Threading.ThreadPool => 0x9a5a3337 => 146
	i32 2592341985, ; 414: Microsoft.Extensions.FileProviders.Abstractions => 0x9a83ffe1 => 200
	i32 2593496499, ; 415: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 326
	i32 2605712449, ; 416: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 305
	i32 2615233544, ; 417: Xamarin.AndroidX.Fragment.Ktx => 0x9be14c08 => 256
	i32 2616218305, ; 418: Microsoft.Extensions.Logging.Debug.dll => 0x9bf052c1 => 208
	i32 2617129537, ; 419: System.Private.Xml.dll => 0x9bfe3a41 => 88
	i32 2618712057, ; 420: System.Reflection.TypeExtensions.dll => 0x9c165ff9 => 96
	i32 2620871830, ; 421: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 246
	i32 2624644809, ; 422: Xamarin.AndroidX.DynamicAnimation => 0x9c70e6c9 => 251
	i32 2626831493, ; 423: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 321
	i32 2627185994, ; 424: System.Diagnostics.TextWriterTraceListener.dll => 0x9c97ad4a => 31
	i32 2629843544, ; 425: System.IO.Compression.ZipFile.dll => 0x9cc03a58 => 45
	i32 2633051222, ; 426: Xamarin.AndroidX.Lifecycle.LiveData => 0x9cf12c56 => 260
	i32 2637500010, ; 427: Microsoft.Extensions.Features => 0x9d350e6a => 199
	i32 2662834856, ; 428: Mapster.dll => 0x9eb7a2a8 => 174
	i32 2663391936, ; 429: Xamarin.Android.Glide.DiskLruCache => 0x9ec022c0 => 225
	i32 2663698177, ; 430: System.Runtime.Loader => 0x9ec4cf01 => 109
	i32 2664396074, ; 431: System.Xml.XDocument.dll => 0x9ecf752a => 158
	i32 2665622720, ; 432: System.Drawing.Primitives => 0x9ee22cc0 => 35
	i32 2676780864, ; 433: System.Data.Common.dll => 0x9f8c6f40 => 22
	i32 2686887180, ; 434: System.Runtime.Serialization.Xml.dll => 0xa026a50c => 114
	i32 2692077919, ; 435: Microsoft.AspNetCore.Components.WebView.dll => 0xa075d95f => 180
	i32 2693849962, ; 436: System.IO.dll => 0xa090e36a => 57
	i32 2701096212, ; 437: Xamarin.AndroidX.Tracing.Tracing => 0xa0ff7514 => 285
	i32 2715334215, ; 438: System.Threading.Tasks.dll => 0xa1d8b647 => 144
	i32 2717744543, ; 439: System.Security.Claims => 0xa1fd7d9f => 118
	i32 2719963679, ; 440: System.Security.Cryptography.Cng.dll => 0xa21f5a1f => 120
	i32 2724373263, ; 441: System.Runtime.Numerics.dll => 0xa262a30f => 110
	i32 2732626843, ; 442: Xamarin.AndroidX.Activity => 0xa2e0939b => 227
	i32 2735172069, ; 443: System.Threading.Channels => 0xa30769e5 => 139
	i32 2735631878, ; 444: Microsoft.AspNetCore.Authorization.dll => 0xa30e6e06 => 176
	i32 2737747696, ; 445: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 233
	i32 2740051746, ; 446: Microsoft.Identity.Client => 0xa351df22 => 212
	i32 2740948882, ; 447: System.IO.Pipes.AccessControl => 0xa35f8f92 => 54
	i32 2748088231, ; 448: System.Runtime.InteropServices.JavaScript => 0xa3cc7fa7 => 105
	i32 2752995522, ; 449: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 327
	i32 2758225723, ; 450: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 218
	i32 2764765095, ; 451: Microsoft.Maui.dll => 0xa4caf7a7 => 219
	i32 2765824710, ; 452: System.Text.Encoding.CodePages.dll => 0xa4db22c6 => 133
	i32 2770495804, ; 453: Xamarin.Jetbrains.Annotations.dll => 0xa522693c => 299
	i32 2778768386, ; 454: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 290
	i32 2779977773, ; 455: Xamarin.AndroidX.ResourceInspection.Annotation.dll => 0xa5b3182d => 278
	i32 2785988530, ; 456: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 333
	i32 2788224221, ; 457: Xamarin.AndroidX.Fragment.Ktx.dll => 0xa630ecdd => 256
	i32 2801831435, ; 458: Microsoft.Maui.Graphics => 0xa7008e0b => 221
	i32 2803228030, ; 459: System.Xml.XPath.XDocument.dll => 0xa715dd7e => 159
	i32 2806116107, ; 460: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 312
	i32 2810250172, ; 461: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 243
	i32 2819470561, ; 462: System.Xml.dll => 0xa80db4e1 => 163
	i32 2821205001, ; 463: System.ServiceProcess.dll => 0xa8282c09 => 132
	i32 2821294376, ; 464: Xamarin.AndroidX.ResourceInspection.Annotation => 0xa8298928 => 278
	i32 2824502124, ; 465: System.Xml.XmlDocument => 0xa85a7b6c => 161
	i32 2831556043, ; 466: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 325
	i32 2833784645, ; 467: Microsoft.AspNetCore.Metadata => 0xa8e81f45 => 185
	i32 2838993487, ; 468: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx.dll => 0xa9379a4f => 267
	i32 2849599387, ; 469: System.Threading.Overlapped.dll => 0xa9d96f9b => 140
	i32 2853208004, ; 470: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 290
	i32 2855708567, ; 471: Xamarin.AndroidX.Transition => 0xaa36a797 => 286
	i32 2861098320, ; 472: Mono.Android.Export.dll => 0xaa88e550 => 169
	i32 2861189240, ; 473: Microsoft.Maui.Essentials => 0xaa8a4878 => 220
	i32 2870099610, ; 474: Xamarin.AndroidX.Activity.Ktx.dll => 0xab123e9a => 228
	i32 2875164099, ; 475: Jsr305Binding.dll => 0xab5f85c3 => 295
	i32 2875220617, ; 476: System.Globalization.Calendars.dll => 0xab606289 => 40
	i32 2875347124, ; 477: Microsoft.AspNetCore.Http.Connections.Client.dll => 0xab6250b4 => 183
	i32 2884993177, ; 478: Xamarin.AndroidX.ExifInterface => 0xabf58099 => 254
	i32 2887636118, ; 479: System.Net.dll => 0xac1dd496 => 81
	i32 2892341533, ; 480: Microsoft.AspNetCore.Components.Web => 0xac65a11d => 179
	i32 2899753641, ; 481: System.IO.UnmanagedMemoryStream => 0xacd6baa9 => 56
	i32 2900621748, ; 482: System.Dynamic.Runtime.dll => 0xace3f9b4 => 37
	i32 2901442782, ; 483: System.Reflection => 0xacf080de => 97
	i32 2905242038, ; 484: mscorlib.dll => 0xad2a79b6 => 166
	i32 2909740682, ; 485: System.Private.CoreLib => 0xad6f1e8a => 172
	i32 2911054922, ; 486: Microsoft.Extensions.FileProviders.Physical.dll => 0xad832c4a => 203
	i32 2916838712, ; 487: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 291
	i32 2919462931, ; 488: System.Numerics.Vectors.dll => 0xae037813 => 82
	i32 2921128767, ; 489: Xamarin.AndroidX.Annotation.Experimental.dll => 0xae1ce33f => 230
	i32 2936416060, ; 490: System.Resources.Reader => 0xaf06273c => 98
	i32 2940926066, ; 491: System.Diagnostics.StackTrace.dll => 0xaf4af872 => 30
	i32 2942453041, ; 492: System.Xml.XPath.XDocument => 0xaf624531 => 159
	i32 2959614098, ; 493: System.ComponentModel.dll => 0xb0682092 => 18
	i32 2968338931, ; 494: System.Security.Principal.Windows => 0xb0ed41f3 => 127
	i32 2971004615, ; 495: Microsoft.Extensions.Options.ConfigurationExtensions.dll => 0xb115eec7 => 210
	i32 2972252294, ; 496: System.Security.Cryptography.Algorithms.dll => 0xb128f886 => 119
	i32 2978675010, ; 497: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 250
	i32 2987532451, ; 498: Xamarin.AndroidX.Security.SecurityCrypto => 0xb21220a3 => 281
	i32 2996846495, ; 499: Xamarin.AndroidX.Lifecycle.Process.dll => 0xb2a03f9f => 263
	i32 3016983068, ; 500: Xamarin.AndroidX.Startup.StartupRuntime => 0xb3d3821c => 283
	i32 3020703001, ; 501: Microsoft.Extensions.Diagnostics => 0xb40c4519 => 197
	i32 3023353419, ; 502: WindowsBase.dll => 0xb434b64b => 165
	i32 3024354802, ; 503: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 258
	i32 3038032645, ; 504: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 340
	i32 3056245963, ; 505: Xamarin.AndroidX.SavedState.SavedState.Ktx => 0xb62a9ccb => 280
	i32 3057625584, ; 506: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 271
	i32 3059408633, ; 507: Mono.Android.Runtime => 0xb65adef9 => 170
	i32 3059793426, ; 508: System.ComponentModel.Primitives => 0xb660be12 => 16
	i32 3075834255, ; 509: System.Threading.Tasks => 0xb755818f => 144
	i32 3077302341, ; 510: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 318
	i32 3090735792, ; 511: System.Security.Cryptography.X509Certificates.dll => 0xb838e2b0 => 125
	i32 3099732863, ; 512: System.Security.Claims.dll => 0xb8c22b7f => 118
	i32 3103600923, ; 513: System.Formats.Asn1 => 0xb8fd311b => 38
	i32 3111772706, ; 514: System.Runtime.Serialization => 0xb979e222 => 115
	i32 3121463068, ; 515: System.IO.FileSystem.AccessControl.dll => 0xba0dbf1c => 47
	i32 3124832203, ; 516: System.Threading.Tasks.Extensions => 0xba4127cb => 142
	i32 3132293585, ; 517: System.Security.AccessControl => 0xbab301d1 => 117
	i32 3147165239, ; 518: System.Diagnostics.Tracing.dll => 0xbb95ee37 => 34
	i32 3148237826, ; 519: GoogleGson.dll => 0xbba64c02 => 173
	i32 3159123045, ; 520: System.Reflection.Primitives.dll => 0xbc4c6465 => 95
	i32 3160747431, ; 521: System.IO.MemoryMappedFiles => 0xbc652da7 => 53
	i32 3178803400, ; 522: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 272
	i32 3192346100, ; 523: System.Security.SecureString => 0xbe4755f4 => 129
	i32 3193515020, ; 524: System.Web => 0xbe592c0c => 153
	i32 3204380047, ; 525: System.Data.dll => 0xbefef58f => 24
	i32 3209718065, ; 526: System.Xml.XmlDocument.dll => 0xbf506931 => 161
	i32 3211777861, ; 527: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 249
	i32 3220365878, ; 528: System.Threading => 0xbff2e236 => 148
	i32 3226221578, ; 529: System.Runtime.Handles.dll => 0xc04c3c0a => 104
	i32 3251039220, ; 530: System.Reflection.DispatchProxy.dll => 0xc1c6ebf4 => 89
	i32 3258312781, ; 531: Xamarin.AndroidX.CardView => 0xc235e84d => 237
	i32 3265493905, ; 532: System.Linq.Queryable.dll => 0xc2a37b91 => 60
	i32 3265893370, ; 533: System.Threading.Tasks.Extensions.dll => 0xc2a993fa => 142
	i32 3277815716, ; 534: System.Resources.Writer.dll => 0xc35f7fa4 => 100
	i32 3279906254, ; 535: Microsoft.Win32.Registry.dll => 0xc37f65ce => 5
	i32 3280506390, ; 536: System.ComponentModel.Annotations.dll => 0xc3888e16 => 13
	i32 3290767353, ; 537: System.Security.Cryptography.Encoding => 0xc4251ff9 => 122
	i32 3299363146, ; 538: System.Text.Encoding => 0xc4a8494a => 135
	i32 3303498502, ; 539: System.Diagnostics.FileVersionInfo => 0xc4e76306 => 28
	i32 3305363605, ; 540: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 313
	i32 3316684772, ; 541: System.Net.Requests.dll => 0xc5b097e4 => 72
	i32 3317135071, ; 542: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 247
	i32 3317144872, ; 543: System.Data => 0xc5b79d28 => 24
	i32 3340431453, ; 544: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 235
	i32 3345895724, ; 545: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller.dll => 0xc76e512c => 276
	i32 3346324047, ; 546: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 273
	i32 3357674450, ; 547: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 330
	i32 3358260929, ; 548: System.Text.Json => 0xc82afec1 => 137
	i32 3362336904, ; 549: Xamarin.AndroidX.Activity.Ktx => 0xc8693088 => 228
	i32 3362522851, ; 550: Xamarin.AndroidX.Core => 0xc86c06e3 => 244
	i32 3366347497, ; 551: Java.Interop => 0xc8a662e9 => 168
	i32 3374999561, ; 552: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 277
	i32 3381016424, ; 553: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 309
	i32 3395150330, ; 554: System.Runtime.CompilerServices.Unsafe.dll => 0xca5de1fa => 101
	i32 3403906625, ; 555: System.Security.Cryptography.OpenSsl.dll => 0xcae37e41 => 123
	i32 3405233483, ; 556: Xamarin.AndroidX.CustomView.PoolingContainer => 0xcaf7bd4b => 248
	i32 3406629867, ; 557: Microsoft.Extensions.FileProviders.Composite.dll => 0xcb0d0beb => 201
	i32 3421170118, ; 558: Microsoft.Extensions.Configuration.Binder => 0xcbeae9c6 => 192
	i32 3428513518, ; 559: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 195
	i32 3429136800, ; 560: System.Xml => 0xcc6479a0 => 163
	i32 3430777524, ; 561: netstandard => 0xcc7d82b4 => 167
	i32 3441283291, ; 562: Xamarin.AndroidX.DynamicAnimation.dll => 0xcd1dd0db => 251
	i32 3445260447, ; 563: System.Formats.Tar => 0xcd5a809f => 39
	i32 3452344032, ; 564: Microsoft.Maui.Controls.Compatibility.dll => 0xcdc696e0 => 216
	i32 3463511458, ; 565: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 317
	i32 3464190856, ; 566: Microsoft.AspNetCore.Components.Forms.dll => 0xce7b5b88 => 178
	i32 3466904072, ; 567: Microsoft.AspNetCore.SignalR.Client.dll => 0xcea4c208 => 186
	i32 3471940407, ; 568: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 17
	i32 3476120550, ; 569: Mono.Android => 0xcf3163e6 => 171
	i32 3479583265, ; 570: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 330
	i32 3484440000, ; 571: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 329
	i32 3485117614, ; 572: System.Text.Json.dll => 0xcfbaacae => 137
	i32 3486566296, ; 573: System.Transactions => 0xcfd0c798 => 150
	i32 3493954962, ; 574: Xamarin.AndroidX.Concurrent.Futures.dll => 0xd0418592 => 240
	i32 3500000672, ; 575: Microsoft.JSInterop => 0xd09dc5a0 => 215
	i32 3509114376, ; 576: System.Xml.Linq => 0xd128d608 => 155
	i32 3515174580, ; 577: System.Security.dll => 0xd1854eb4 => 130
	i32 3530912306, ; 578: System.Configuration => 0xd2757232 => 19
	i32 3539954161, ; 579: System.Net.HttpListener => 0xd2ff69f1 => 65
	i32 3560100363, ; 580: System.Threading.Timer => 0xd432d20b => 147
	i32 3570554715, ; 581: System.IO.FileSystem.AccessControl => 0xd4d2575b => 47
	i32 3580758918, ; 582: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 337
	i32 3597029428, ; 583: Xamarin.Android.Glide.GifDecoder.dll => 0xd6665034 => 226
	i32 3598340787, ; 584: System.Net.WebSockets.Client => 0xd67a52b3 => 79
	i32 3608519521, ; 585: System.Linq.dll => 0xd715a361 => 61
	i32 3624195450, ; 586: System.Runtime.InteropServices.RuntimeInformation => 0xd804d57a => 106
	i32 3627220390, ; 587: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 275
	i32 3633644679, ; 588: Xamarin.AndroidX.Annotation.Experimental => 0xd8950487 => 230
	i32 3638274909, ; 589: System.IO.FileSystem.Primitives.dll => 0xd8dbab5d => 49
	i32 3641597786, ; 590: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 261
	i32 3643446276, ; 591: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 334
	i32 3643854240, ; 592: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 272
	i32 3645089577, ; 593: System.ComponentModel.DataAnnotations => 0xd943a729 => 14
	i32 3657292374, ; 594: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 191
	i32 3660523487, ; 595: System.Net.NetworkInformation => 0xda2f27df => 68
	i32 3672681054, ; 596: Mono.Android.dll => 0xdae8aa5e => 171
	i32 3682565725, ; 597: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 236
	i32 3684561358, ; 598: Xamarin.AndroidX.Concurrent.Futures => 0xdb9df1ce => 240
	i32 3691870036, ; 599: Microsoft.AspNetCore.SignalR.Protocols.Json => 0xdc0d7754 => 189
	i32 3697841164, ; 600: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 339
	i32 3700591436, ; 601: Microsoft.IdentityModel.Abstractions.dll => 0xdc928b4c => 214
	i32 3700866549, ; 602: System.Net.WebProxy.dll => 0xdc96bdf5 => 78
	i32 3706696989, ; 603: Xamarin.AndroidX.Core.Core.Ktx.dll => 0xdcefb51d => 245
	i32 3716563718, ; 604: System.Runtime.Intrinsics => 0xdd864306 => 108
	i32 3718780102, ; 605: Xamarin.AndroidX.Annotation => 0xdda814c6 => 229
	i32 3722202641, ; 606: Microsoft.Extensions.Configuration.Json.dll => 0xdddc4e11 => 194
	i32 3724971120, ; 607: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 271
	i32 3732100267, ; 608: System.Net.NameResolution => 0xde7354ab => 67
	i32 3732214720, ; 609: Microsoft.AspNetCore.Metadata.dll => 0xde7513c0 => 185
	i32 3737834244, ; 610: System.Net.Http.Json.dll => 0xdecad304 => 63
	i32 3748608112, ; 611: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 27
	i32 3751444290, ; 612: System.Xml.XPath => 0xdf9a7f42 => 160
	i32 3758424670, ; 613: Microsoft.Extensions.Configuration.FileExtensions => 0xe005025e => 193
	i32 3786282454, ; 614: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 238
	i32 3787005001, ; 615: Microsoft.AspNetCore.Connections.Abstractions => 0xe1b91c49 => 182
	i32 3792276235, ; 616: System.Collections.NonGeneric => 0xe2098b0b => 10
	i32 3800979733, ; 617: Microsoft.Maui.Controls.Compatibility => 0xe28e5915 => 216
	i32 3802395368, ; 618: System.Collections.Specialized.dll => 0xe2a3f2e8 => 11
	i32 3819260425, ; 619: System.Net.WebProxy => 0xe3a54a09 => 78
	i32 3823082795, ; 620: System.Security.Cryptography.dll => 0xe3df9d2b => 126
	i32 3829621856, ; 621: System.Numerics.dll => 0xe4436460 => 83
	i32 3841636137, ; 622: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 196
	i32 3844307129, ; 623: System.Net.Mail.dll => 0xe52378b9 => 66
	i32 3849253459, ; 624: System.Runtime.InteropServices.dll => 0xe56ef253 => 107
	i32 3870376305, ; 625: System.Net.HttpListener.dll => 0xe6b14171 => 65
	i32 3873536506, ; 626: System.Security.Principal => 0xe6e179fa => 128
	i32 3875112723, ; 627: System.Security.Cryptography.Encoding.dll => 0xe6f98713 => 122
	i32 3885497537, ; 628: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 77
	i32 3885922214, ; 629: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 286
	i32 3888767677, ; 630: Xamarin.AndroidX.ProfileInstaller.ProfileInstaller => 0xe7c9e2bd => 276
	i32 3889960447, ; 631: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 338
	i32 3896106733, ; 632: System.Collections.Concurrent.dll => 0xe839deed => 8
	i32 3896760992, ; 633: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 244
	i32 3901907137, ; 634: Microsoft.VisualBasic.Core.dll => 0xe89260c1 => 2
	i32 3920810846, ; 635: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 44
	i32 3921031405, ; 636: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 289
	i32 3928044579, ; 637: System.Xml.ReaderWriter => 0xea213423 => 156
	i32 3930554604, ; 638: System.Security.Principal.dll => 0xea4780ec => 128
	i32 3931092270, ; 639: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 274
	i32 3945713374, ; 640: System.Data.DataSetExtensions.dll => 0xeb2ecede => 23
	i32 3953953790, ; 641: System.Text.Encoding.CodePages => 0xebac8bfe => 133
	i32 3955647286, ; 642: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 232
	i32 3959773229, ; 643: Xamarin.AndroidX.Lifecycle.Process => 0xec05582d => 263
	i32 3980434154, ; 644: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 333
	i32 3987592930, ; 645: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 315
	i32 4003436829, ; 646: System.Diagnostics.Process.dll => 0xee9f991d => 29
	i32 4015948917, ; 647: Xamarin.AndroidX.Annotation.Jvm.dll => 0xef5e8475 => 231
	i32 4023392905, ; 648: System.IO.Pipelines => 0xefd01a89 => 222
	i32 4025784931, ; 649: System.Memory => 0xeff49a63 => 62
	i32 4046471985, ; 650: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 218
	i32 4054681211, ; 651: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 90
	i32 4068434129, ; 652: System.Private.Xml.Linq.dll => 0xf27f60d1 => 87
	i32 4073602200, ; 653: System.Threading.dll => 0xf2ce3c98 => 148
	i32 4094352644, ; 654: Microsoft.Maui.Essentials.dll => 0xf40add04 => 220
	i32 4099507663, ; 655: System.Drawing.dll => 0xf45985cf => 36
	i32 4100113165, ; 656: System.Private.Uri => 0xf462c30d => 86
	i32 4101593132, ; 657: Xamarin.AndroidX.Emoji2 => 0xf479582c => 252
	i32 4102112229, ; 658: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 328
	i32 4125707920, ; 659: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 323
	i32 4126470640, ; 660: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 195
	i32 4127667938, ; 661: System.IO.FileSystem.Watcher => 0xf60736e2 => 50
	i32 4130442656, ; 662: System.AppContext => 0xf6318da0 => 6
	i32 4147896353, ; 663: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 90
	i32 4150914736, ; 664: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 335
	i32 4151237749, ; 665: System.Core => 0xf76edc75 => 21
	i32 4159265925, ; 666: System.Xml.XmlSerializer => 0xf7e95c85 => 162
	i32 4161255271, ; 667: System.Reflection.TypeExtensions => 0xf807b767 => 96
	i32 4164802419, ; 668: System.IO.FileSystem.Watcher.dll => 0xf83dd773 => 50
	i32 4181436372, ; 669: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 113
	i32 4182413190, ; 670: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 268
	i32 4185676441, ; 671: System.Security => 0xf97c5a99 => 130
	i32 4196529839, ; 672: System.Net.WebClient.dll => 0xfa21f6af => 76
	i32 4213026141, ; 673: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 27
	i32 4256097574, ; 674: Xamarin.AndroidX.Core.Core.Ktx => 0xfdaee526 => 245
	i32 4258378803, ; 675: Xamarin.AndroidX.Lifecycle.ViewModel.Ktx => 0xfdd1b433 => 267
	i32 4260525087, ; 676: System.Buffers => 0xfdf2741f => 7
	i32 4271975918, ; 677: Microsoft.Maui.Controls.dll => 0xfea12dee => 217
	i32 4274976490, ; 678: System.Runtime.Numerics => 0xfecef6ea => 110
	i32 4292120959, ; 679: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 268
	i32 4294648842, ; 680: Microsoft.Extensions.FileProviders.Embedded => 0xfffb240a => 202
	i32 4294763496 ; 681: Xamarin.AndroidX.ExifInterface.dll => 0xfffce3e8 => 254
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [682 x i32] [
	i32 68, ; 0
	i32 67, ; 1
	i32 108, ; 2
	i32 264, ; 3
	i32 298, ; 4
	i32 48, ; 5
	i32 80, ; 6
	i32 145, ; 7
	i32 30, ; 8
	i32 339, ; 9
	i32 124, ; 10
	i32 221, ; 11
	i32 102, ; 12
	i32 198, ; 13
	i32 282, ; 14
	i32 107, ; 15
	i32 282, ; 16
	i32 139, ; 17
	i32 302, ; 18
	i32 77, ; 19
	i32 124, ; 20
	i32 13, ; 21
	i32 238, ; 22
	i32 132, ; 23
	i32 284, ; 24
	i32 151, ; 25
	i32 336, ; 26
	i32 337, ; 27
	i32 18, ; 28
	i32 236, ; 29
	i32 26, ; 30
	i32 183, ; 31
	i32 197, ; 32
	i32 258, ; 33
	i32 1, ; 34
	i32 59, ; 35
	i32 42, ; 36
	i32 91, ; 37
	i32 177, ; 38
	i32 241, ; 39
	i32 147, ; 40
	i32 260, ; 41
	i32 257, ; 42
	i32 308, ; 43
	i32 54, ; 44
	i32 205, ; 45
	i32 69, ; 46
	i32 336, ; 47
	i32 227, ; 48
	i32 83, ; 49
	i32 321, ; 50
	i32 259, ; 51
	i32 184, ; 52
	i32 320, ; 53
	i32 131, ; 54
	i32 55, ; 55
	i32 149, ; 56
	i32 74, ; 57
	i32 145, ; 58
	i32 62, ; 59
	i32 146, ; 60
	i32 340, ; 61
	i32 165, ; 62
	i32 332, ; 63
	i32 242, ; 64
	i32 12, ; 65
	i32 255, ; 66
	i32 125, ; 67
	i32 152, ; 68
	i32 188, ; 69
	i32 113, ; 70
	i32 166, ; 71
	i32 164, ; 72
	i32 257, ; 73
	i32 214, ; 74
	i32 270, ; 75
	i32 84, ; 76
	i32 319, ; 77
	i32 313, ; 78
	i32 211, ; 79
	i32 150, ; 80
	i32 302, ; 81
	i32 60, ; 82
	i32 206, ; 83
	i32 51, ; 84
	i32 103, ; 85
	i32 114, ; 86
	i32 40, ; 87
	i32 295, ; 88
	i32 293, ; 89
	i32 202, ; 90
	i32 120, ; 91
	i32 327, ; 92
	i32 0, ; 93
	i32 52, ; 94
	i32 44, ; 95
	i32 119, ; 96
	i32 247, ; 97
	i32 325, ; 98
	i32 253, ; 99
	i32 81, ; 100
	i32 136, ; 101
	i32 289, ; 102
	i32 234, ; 103
	i32 8, ; 104
	i32 73, ; 105
	i32 307, ; 106
	i32 155, ; 107
	i32 304, ; 108
	i32 154, ; 109
	i32 92, ; 110
	i32 299, ; 111
	i32 45, ; 112
	i32 322, ; 113
	i32 310, ; 114
	i32 303, ; 115
	i32 109, ; 116
	i32 210, ; 117
	i32 129, ; 118
	i32 25, ; 119
	i32 224, ; 120
	i32 72, ; 121
	i32 55, ; 122
	i32 46, ; 123
	i32 331, ; 124
	i32 209, ; 125
	i32 248, ; 126
	i32 181, ; 127
	i32 22, ; 128
	i32 262, ; 129
	i32 86, ; 130
	i32 43, ; 131
	i32 160, ; 132
	i32 189, ; 133
	i32 71, ; 134
	i32 275, ; 135
	i32 3, ; 136
	i32 42, ; 137
	i32 63, ; 138
	i32 16, ; 139
	i32 53, ; 140
	i32 334, ; 141
	i32 298, ; 142
	i32 174, ; 143
	i32 105, ; 144
	i32 303, ; 145
	i32 296, ; 146
	i32 259, ; 147
	i32 34, ; 148
	i32 158, ; 149
	i32 85, ; 150
	i32 32, ; 151
	i32 12, ; 152
	i32 51, ; 153
	i32 204, ; 154
	i32 56, ; 155
	i32 279, ; 156
	i32 36, ; 157
	i32 196, ; 158
	i32 309, ; 159
	i32 297, ; 160
	i32 232, ; 161
	i32 35, ; 162
	i32 175, ; 163
	i32 58, ; 164
	i32 198, ; 165
	i32 266, ; 166
	i32 184, ; 167
	i32 213, ; 168
	i32 173, ; 169
	i32 17, ; 170
	i32 300, ; 171
	i32 164, ; 172
	i32 193, ; 173
	i32 322, ; 174
	i32 265, ; 175
	i32 208, ; 176
	i32 292, ; 177
	i32 328, ; 178
	i32 153, ; 179
	i32 200, ; 180
	i32 288, ; 181
	i32 273, ; 182
	i32 326, ; 183
	i32 234, ; 184
	i32 29, ; 185
	i32 52, ; 186
	i32 187, ; 187
	i32 324, ; 188
	i32 293, ; 189
	i32 5, ; 190
	i32 308, ; 191
	i32 283, ; 192
	i32 287, ; 193
	i32 239, ; 194
	i32 304, ; 195
	i32 231, ; 196
	i32 250, ; 197
	i32 85, ; 198
	i32 292, ; 199
	i32 61, ; 200
	i32 112, ; 201
	i32 57, ; 202
	i32 338, ; 203
	i32 279, ; 204
	i32 99, ; 205
	i32 19, ; 206
	i32 243, ; 207
	i32 111, ; 208
	i32 101, ; 209
	i32 182, ; 210
	i32 102, ; 211
	i32 306, ; 212
	i32 104, ; 213
	i32 296, ; 214
	i32 71, ; 215
	i32 38, ; 216
	i32 32, ; 217
	i32 201, ; 218
	i32 103, ; 219
	i32 73, ; 220
	i32 312, ; 221
	i32 9, ; 222
	i32 123, ; 223
	i32 46, ; 224
	i32 233, ; 225
	i32 211, ; 226
	i32 9, ; 227
	i32 43, ; 228
	i32 4, ; 229
	i32 280, ; 230
	i32 316, ; 231
	i32 205, ; 232
	i32 311, ; 233
	i32 204, ; 234
	i32 31, ; 235
	i32 138, ; 236
	i32 92, ; 237
	i32 181, ; 238
	i32 93, ; 239
	i32 331, ; 240
	i32 49, ; 241
	i32 141, ; 242
	i32 112, ; 243
	i32 140, ; 244
	i32 249, ; 245
	i32 115, ; 246
	i32 297, ; 247
	i32 157, ; 248
	i32 76, ; 249
	i32 79, ; 250
	i32 269, ; 251
	i32 37, ; 252
	i32 291, ; 253
	i32 194, ; 254
	i32 253, ; 255
	i32 246, ; 256
	i32 64, ; 257
	i32 138, ; 258
	i32 15, ; 259
	i32 180, ; 260
	i32 116, ; 261
	i32 285, ; 262
	i32 294, ; 263
	i32 241, ; 264
	i32 48, ; 265
	i32 70, ; 266
	i32 80, ; 267
	i32 126, ; 268
	i32 94, ; 269
	i32 121, ; 270
	i32 301, ; 271
	i32 26, ; 272
	i32 262, ; 273
	i32 97, ; 274
	i32 28, ; 275
	i32 237, ; 276
	i32 329, ; 277
	i32 307, ; 278
	i32 149, ; 279
	i32 222, ; 280
	i32 169, ; 281
	i32 4, ; 282
	i32 98, ; 283
	i32 179, ; 284
	i32 33, ; 285
	i32 93, ; 286
	i32 284, ; 287
	i32 206, ; 288
	i32 21, ; 289
	i32 41, ; 290
	i32 170, ; 291
	i32 323, ; 292
	i32 255, ; 293
	i32 315, ; 294
	i32 212, ; 295
	i32 175, ; 296
	i32 269, ; 297
	i32 300, ; 298
	i32 294, ; 299
	i32 274, ; 300
	i32 2, ; 301
	i32 134, ; 302
	i32 111, ; 303
	i32 207, ; 304
	i32 335, ; 305
	i32 224, ; 306
	i32 332, ; 307
	i32 58, ; 308
	i32 95, ; 309
	i32 314, ; 310
	i32 39, ; 311
	i32 235, ; 312
	i32 25, ; 313
	i32 94, ; 314
	i32 89, ; 315
	i32 99, ; 316
	i32 10, ; 317
	i32 87, ; 318
	i32 187, ; 319
	i32 100, ; 320
	i32 281, ; 321
	i32 188, ; 322
	i32 190, ; 323
	i32 301, ; 324
	i32 226, ; 325
	i32 311, ; 326
	i32 7, ; 327
	i32 266, ; 328
	i32 306, ; 329
	i32 223, ; 330
	i32 213, ; 331
	i32 88, ; 332
	i32 192, ; 333
	i32 261, ; 334
	i32 154, ; 335
	i32 310, ; 336
	i32 33, ; 337
	i32 203, ; 338
	i32 116, ; 339
	i32 82, ; 340
	i32 20, ; 341
	i32 11, ; 342
	i32 162, ; 343
	i32 3, ; 344
	i32 219, ; 345
	i32 318, ; 346
	i32 209, ; 347
	i32 207, ; 348
	i32 84, ; 349
	i32 305, ; 350
	i32 64, ; 351
	i32 320, ; 352
	i32 288, ; 353
	i32 143, ; 354
	i32 199, ; 355
	i32 270, ; 356
	i32 157, ; 357
	i32 41, ; 358
	i32 117, ; 359
	i32 191, ; 360
	i32 225, ; 361
	i32 314, ; 362
	i32 277, ; 363
	i32 131, ; 364
	i32 75, ; 365
	i32 66, ; 366
	i32 324, ; 367
	i32 172, ; 368
	i32 229, ; 369
	i32 186, ; 370
	i32 143, ; 371
	i32 106, ; 372
	i32 151, ; 373
	i32 70, ; 374
	i32 156, ; 375
	i32 190, ; 376
	i32 121, ; 377
	i32 127, ; 378
	i32 319, ; 379
	i32 152, ; 380
	i32 252, ; 381
	i32 177, ; 382
	i32 141, ; 383
	i32 239, ; 384
	i32 316, ; 385
	i32 20, ; 386
	i32 14, ; 387
	i32 215, ; 388
	i32 135, ; 389
	i32 75, ; 390
	i32 59, ; 391
	i32 242, ; 392
	i32 167, ; 393
	i32 168, ; 394
	i32 217, ; 395
	i32 15, ; 396
	i32 74, ; 397
	i32 6, ; 398
	i32 23, ; 399
	i32 264, ; 400
	i32 223, ; 401
	i32 0, ; 402
	i32 176, ; 403
	i32 91, ; 404
	i32 317, ; 405
	i32 1, ; 406
	i32 136, ; 407
	i32 265, ; 408
	i32 287, ; 409
	i32 134, ; 410
	i32 69, ; 411
	i32 178, ; 412
	i32 146, ; 413
	i32 200, ; 414
	i32 326, ; 415
	i32 305, ; 416
	i32 256, ; 417
	i32 208, ; 418
	i32 88, ; 419
	i32 96, ; 420
	i32 246, ; 421
	i32 251, ; 422
	i32 321, ; 423
	i32 31, ; 424
	i32 45, ; 425
	i32 260, ; 426
	i32 199, ; 427
	i32 174, ; 428
	i32 225, ; 429
	i32 109, ; 430
	i32 158, ; 431
	i32 35, ; 432
	i32 22, ; 433
	i32 114, ; 434
	i32 180, ; 435
	i32 57, ; 436
	i32 285, ; 437
	i32 144, ; 438
	i32 118, ; 439
	i32 120, ; 440
	i32 110, ; 441
	i32 227, ; 442
	i32 139, ; 443
	i32 176, ; 444
	i32 233, ; 445
	i32 212, ; 446
	i32 54, ; 447
	i32 105, ; 448
	i32 327, ; 449
	i32 218, ; 450
	i32 219, ; 451
	i32 133, ; 452
	i32 299, ; 453
	i32 290, ; 454
	i32 278, ; 455
	i32 333, ; 456
	i32 256, ; 457
	i32 221, ; 458
	i32 159, ; 459
	i32 312, ; 460
	i32 243, ; 461
	i32 163, ; 462
	i32 132, ; 463
	i32 278, ; 464
	i32 161, ; 465
	i32 325, ; 466
	i32 185, ; 467
	i32 267, ; 468
	i32 140, ; 469
	i32 290, ; 470
	i32 286, ; 471
	i32 169, ; 472
	i32 220, ; 473
	i32 228, ; 474
	i32 295, ; 475
	i32 40, ; 476
	i32 183, ; 477
	i32 254, ; 478
	i32 81, ; 479
	i32 179, ; 480
	i32 56, ; 481
	i32 37, ; 482
	i32 97, ; 483
	i32 166, ; 484
	i32 172, ; 485
	i32 203, ; 486
	i32 291, ; 487
	i32 82, ; 488
	i32 230, ; 489
	i32 98, ; 490
	i32 30, ; 491
	i32 159, ; 492
	i32 18, ; 493
	i32 127, ; 494
	i32 210, ; 495
	i32 119, ; 496
	i32 250, ; 497
	i32 281, ; 498
	i32 263, ; 499
	i32 283, ; 500
	i32 197, ; 501
	i32 165, ; 502
	i32 258, ; 503
	i32 340, ; 504
	i32 280, ; 505
	i32 271, ; 506
	i32 170, ; 507
	i32 16, ; 508
	i32 144, ; 509
	i32 318, ; 510
	i32 125, ; 511
	i32 118, ; 512
	i32 38, ; 513
	i32 115, ; 514
	i32 47, ; 515
	i32 142, ; 516
	i32 117, ; 517
	i32 34, ; 518
	i32 173, ; 519
	i32 95, ; 520
	i32 53, ; 521
	i32 272, ; 522
	i32 129, ; 523
	i32 153, ; 524
	i32 24, ; 525
	i32 161, ; 526
	i32 249, ; 527
	i32 148, ; 528
	i32 104, ; 529
	i32 89, ; 530
	i32 237, ; 531
	i32 60, ; 532
	i32 142, ; 533
	i32 100, ; 534
	i32 5, ; 535
	i32 13, ; 536
	i32 122, ; 537
	i32 135, ; 538
	i32 28, ; 539
	i32 313, ; 540
	i32 72, ; 541
	i32 247, ; 542
	i32 24, ; 543
	i32 235, ; 544
	i32 276, ; 545
	i32 273, ; 546
	i32 330, ; 547
	i32 137, ; 548
	i32 228, ; 549
	i32 244, ; 550
	i32 168, ; 551
	i32 277, ; 552
	i32 309, ; 553
	i32 101, ; 554
	i32 123, ; 555
	i32 248, ; 556
	i32 201, ; 557
	i32 192, ; 558
	i32 195, ; 559
	i32 163, ; 560
	i32 167, ; 561
	i32 251, ; 562
	i32 39, ; 563
	i32 216, ; 564
	i32 317, ; 565
	i32 178, ; 566
	i32 186, ; 567
	i32 17, ; 568
	i32 171, ; 569
	i32 330, ; 570
	i32 329, ; 571
	i32 137, ; 572
	i32 150, ; 573
	i32 240, ; 574
	i32 215, ; 575
	i32 155, ; 576
	i32 130, ; 577
	i32 19, ; 578
	i32 65, ; 579
	i32 147, ; 580
	i32 47, ; 581
	i32 337, ; 582
	i32 226, ; 583
	i32 79, ; 584
	i32 61, ; 585
	i32 106, ; 586
	i32 275, ; 587
	i32 230, ; 588
	i32 49, ; 589
	i32 261, ; 590
	i32 334, ; 591
	i32 272, ; 592
	i32 14, ; 593
	i32 191, ; 594
	i32 68, ; 595
	i32 171, ; 596
	i32 236, ; 597
	i32 240, ; 598
	i32 189, ; 599
	i32 339, ; 600
	i32 214, ; 601
	i32 78, ; 602
	i32 245, ; 603
	i32 108, ; 604
	i32 229, ; 605
	i32 194, ; 606
	i32 271, ; 607
	i32 67, ; 608
	i32 185, ; 609
	i32 63, ; 610
	i32 27, ; 611
	i32 160, ; 612
	i32 193, ; 613
	i32 238, ; 614
	i32 182, ; 615
	i32 10, ; 616
	i32 216, ; 617
	i32 11, ; 618
	i32 78, ; 619
	i32 126, ; 620
	i32 83, ; 621
	i32 196, ; 622
	i32 66, ; 623
	i32 107, ; 624
	i32 65, ; 625
	i32 128, ; 626
	i32 122, ; 627
	i32 77, ; 628
	i32 286, ; 629
	i32 276, ; 630
	i32 338, ; 631
	i32 8, ; 632
	i32 244, ; 633
	i32 2, ; 634
	i32 44, ; 635
	i32 289, ; 636
	i32 156, ; 637
	i32 128, ; 638
	i32 274, ; 639
	i32 23, ; 640
	i32 133, ; 641
	i32 232, ; 642
	i32 263, ; 643
	i32 333, ; 644
	i32 315, ; 645
	i32 29, ; 646
	i32 231, ; 647
	i32 222, ; 648
	i32 62, ; 649
	i32 218, ; 650
	i32 90, ; 651
	i32 87, ; 652
	i32 148, ; 653
	i32 220, ; 654
	i32 36, ; 655
	i32 86, ; 656
	i32 252, ; 657
	i32 328, ; 658
	i32 323, ; 659
	i32 195, ; 660
	i32 50, ; 661
	i32 6, ; 662
	i32 90, ; 663
	i32 335, ; 664
	i32 21, ; 665
	i32 162, ; 666
	i32 96, ; 667
	i32 50, ; 668
	i32 113, ; 669
	i32 268, ; 670
	i32 130, ; 671
	i32 76, ; 672
	i32 27, ; 673
	i32 245, ; 674
	i32 267, ; 675
	i32 7, ; 676
	i32 217, ; 677
	i32 110, ; 678
	i32 268, ; 679
	i32 202, ; 680
	i32 254 ; 681
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+armv7-a,+d32,+dsp,+fp64,+neon,+vfp2,+vfp2sp,+vfp3,+vfp3d16,+vfp3d16sp,+vfp3sp,-aes,-fp-armv8,-fp-armv8d16,-fp-armv8d16sp,-fp-armv8sp,-fp16,-fp16fml,-fullfp16,-sha2,-thumb-mode,-vfp4,-vfp4d16,-vfp4d16sp,-vfp4sp" }

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"min_enum_size", i32 4}
