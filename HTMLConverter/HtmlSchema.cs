using System.Collections;

namespace HTMLConverter
{
	internal class HtmlSchema
	{
		private static ArrayList _htmlInlineElements;

		private static ArrayList _htmlBlockElements;

		private static ArrayList _htmlOtherOpenableElements;

		private static ArrayList _htmlEmptyElements;

		private static ArrayList _htmlElementsClosingOnParentElementEnd;

		private static ArrayList _htmlElementsClosingColgroup;

		private static ArrayList _htmlElementsClosingDD;

		private static ArrayList _htmlElementsClosingDT;

		private static ArrayList _htmlElementsClosingLI;

		private static ArrayList _htmlElementsClosingTbody;

		private static ArrayList _htmlElementsClosingTD;

		private static ArrayList _htmlElementsClosingTfoot;

		private static ArrayList _htmlElementsClosingThead;

		private static ArrayList _htmlElementsClosingTH;

		private static ArrayList _htmlElementsClosingTR;

		private static Hashtable _htmlCharacterEntities;

		static HtmlSchema()
		{
			InitializeInlineElements();
			InitializeBlockElements();
			InitializeOtherOpenableElements();
			InitializeEmptyElements();
			InitializeElementsClosingOnParentElementEnd();
			InitializeElementsClosingOnNewElementStart();
			InitializeHtmlCharacterEntities();
		}

		internal static bool IsEmptyElement(string xmlElementName)
		{
			return _htmlEmptyElements.Contains(xmlElementName.ToLower());
		}

		internal static bool IsBlockElement(string xmlElementName)
		{
			return _htmlBlockElements.Contains(xmlElementName);
		}

		internal static bool IsInlineElement(string xmlElementName)
		{
			return _htmlInlineElements.Contains(xmlElementName);
		}

		internal static bool IsKnownOpenableElement(string xmlElementName)
		{
			return _htmlOtherOpenableElements.Contains(xmlElementName);
		}

		internal static bool ClosesOnParentElementEnd(string xmlElementName)
		{
			return _htmlElementsClosingOnParentElementEnd.Contains(xmlElementName.ToLower());
		}

		internal static bool ClosesOnNextElementStart(string currentElementName, string nextElementName)
		{
			switch (currentElementName)
			{
			case "colgroup":
				if (_htmlElementsClosingColgroup.Contains(nextElementName))
				{
					return IsBlockElement(nextElementName);
				}
				return false;
			case "dd":
				if (_htmlElementsClosingDD.Contains(nextElementName))
				{
					return IsBlockElement(nextElementName);
				}
				return false;
			case "dt":
				if (_htmlElementsClosingDT.Contains(nextElementName))
				{
					return IsBlockElement(nextElementName);
				}
				return false;
			case "li":
				return _htmlElementsClosingLI.Contains(nextElementName);
			case "p":
				return IsBlockElement(nextElementName);
			case "tbody":
				return _htmlElementsClosingTbody.Contains(nextElementName);
			case "tfoot":
				return _htmlElementsClosingTfoot.Contains(nextElementName);
			case "thead":
				return _htmlElementsClosingThead.Contains(nextElementName);
			case "tr":
				return _htmlElementsClosingTR.Contains(nextElementName);
			case "td":
				return _htmlElementsClosingTD.Contains(nextElementName);
			case "th":
				return _htmlElementsClosingTH.Contains(nextElementName);
			default:
				return false;
			}
		}

		internal static bool IsEntity(string entityName)
		{
			if (_htmlCharacterEntities.Contains(entityName))
			{
				return true;
			}
			return false;
		}

		internal static char EntityCharacterValue(string entityName)
		{
			if (_htmlCharacterEntities.Contains(entityName))
			{
				return (char)_htmlCharacterEntities[entityName];
			}
			return '\0';
		}

		private static void InitializeInlineElements()
		{
			_htmlInlineElements = new ArrayList();
			_htmlInlineElements.Add("a");
			_htmlInlineElements.Add("abbr");
			_htmlInlineElements.Add("acronym");
			_htmlInlineElements.Add("address");
			_htmlInlineElements.Add("b");
			_htmlInlineElements.Add("bdo");
			_htmlInlineElements.Add("big");
			_htmlInlineElements.Add("button");
			_htmlInlineElements.Add("code");
			_htmlInlineElements.Add("del");
			_htmlInlineElements.Add("dfn");
			_htmlInlineElements.Add("em");
			_htmlInlineElements.Add("font");
			_htmlInlineElements.Add("i");
			_htmlInlineElements.Add("ins");
			_htmlInlineElements.Add("kbd");
			_htmlInlineElements.Add("label");
			_htmlInlineElements.Add("legend");
			_htmlInlineElements.Add("q");
			_htmlInlineElements.Add("s");
			_htmlInlineElements.Add("samp");
			_htmlInlineElements.Add("small");
			_htmlInlineElements.Add("span");
			_htmlInlineElements.Add("strike");
			_htmlInlineElements.Add("strong");
			_htmlInlineElements.Add("sub");
			_htmlInlineElements.Add("sup");
			_htmlInlineElements.Add("u");
			_htmlInlineElements.Add("var");
		}

		private static void InitializeBlockElements()
		{
			_htmlBlockElements = new ArrayList();
			_htmlBlockElements.Add("blockquote");
			_htmlBlockElements.Add("body");
			_htmlBlockElements.Add("caption");
			_htmlBlockElements.Add("center");
			_htmlBlockElements.Add("cite");
			_htmlBlockElements.Add("dd");
			_htmlBlockElements.Add("dir");
			_htmlBlockElements.Add("div");
			_htmlBlockElements.Add("dl");
			_htmlBlockElements.Add("dt");
			_htmlBlockElements.Add("form");
			_htmlBlockElements.Add("h1");
			_htmlBlockElements.Add("h2");
			_htmlBlockElements.Add("h3");
			_htmlBlockElements.Add("h4");
			_htmlBlockElements.Add("h5");
			_htmlBlockElements.Add("h6");
			_htmlBlockElements.Add("html");
			_htmlBlockElements.Add("li");
			_htmlBlockElements.Add("menu");
			_htmlBlockElements.Add("ol");
			_htmlBlockElements.Add("p");
			_htmlBlockElements.Add("pre");
			_htmlBlockElements.Add("table");
			_htmlBlockElements.Add("tbody");
			_htmlBlockElements.Add("td");
			_htmlBlockElements.Add("textarea");
			_htmlBlockElements.Add("tfoot");
			_htmlBlockElements.Add("th");
			_htmlBlockElements.Add("thead");
			_htmlBlockElements.Add("tr");
			_htmlBlockElements.Add("tt");
			_htmlBlockElements.Add("ul");
		}

		private static void InitializeEmptyElements()
		{
			_htmlEmptyElements = new ArrayList();
			_htmlEmptyElements.Add("area");
			_htmlEmptyElements.Add("base");
			_htmlEmptyElements.Add("basefont");
			_htmlEmptyElements.Add("br");
			_htmlEmptyElements.Add("col");
			_htmlEmptyElements.Add("frame");
			_htmlEmptyElements.Add("hr");
			_htmlEmptyElements.Add("img");
			_htmlEmptyElements.Add("input");
			_htmlEmptyElements.Add("isindex");
			_htmlEmptyElements.Add("link");
			_htmlEmptyElements.Add("meta");
			_htmlEmptyElements.Add("param");
		}

		private static void InitializeOtherOpenableElements()
		{
			_htmlOtherOpenableElements = new ArrayList();
			_htmlOtherOpenableElements.Add("applet");
			_htmlOtherOpenableElements.Add("base");
			_htmlOtherOpenableElements.Add("basefont");
			_htmlOtherOpenableElements.Add("colgroup");
			_htmlOtherOpenableElements.Add("fieldset");
			_htmlOtherOpenableElements.Add("frameset");
			_htmlOtherOpenableElements.Add("head");
			_htmlOtherOpenableElements.Add("iframe");
			_htmlOtherOpenableElements.Add("map");
			_htmlOtherOpenableElements.Add("noframes");
			_htmlOtherOpenableElements.Add("noscript");
			_htmlOtherOpenableElements.Add("object");
			_htmlOtherOpenableElements.Add("optgroup");
			_htmlOtherOpenableElements.Add("option");
			_htmlOtherOpenableElements.Add("script");
			_htmlOtherOpenableElements.Add("select");
			_htmlOtherOpenableElements.Add("style");
			_htmlOtherOpenableElements.Add("title");
		}

		private static void InitializeElementsClosingOnParentElementEnd()
		{
			_htmlElementsClosingOnParentElementEnd = new ArrayList();
			_htmlElementsClosingOnParentElementEnd.Add("body");
			_htmlElementsClosingOnParentElementEnd.Add("colgroup");
			_htmlElementsClosingOnParentElementEnd.Add("dd");
			_htmlElementsClosingOnParentElementEnd.Add("dt");
			_htmlElementsClosingOnParentElementEnd.Add("head");
			_htmlElementsClosingOnParentElementEnd.Add("html");
			_htmlElementsClosingOnParentElementEnd.Add("li");
			_htmlElementsClosingOnParentElementEnd.Add("p");
			_htmlElementsClosingOnParentElementEnd.Add("tbody");
			_htmlElementsClosingOnParentElementEnd.Add("td");
			_htmlElementsClosingOnParentElementEnd.Add("tfoot");
			_htmlElementsClosingOnParentElementEnd.Add("thead");
			_htmlElementsClosingOnParentElementEnd.Add("th");
			_htmlElementsClosingOnParentElementEnd.Add("tr");
		}

		private static void InitializeElementsClosingOnNewElementStart()
		{
			_htmlElementsClosingColgroup = new ArrayList();
			_htmlElementsClosingColgroup.Add("colgroup");
			_htmlElementsClosingColgroup.Add("tr");
			_htmlElementsClosingColgroup.Add("thead");
			_htmlElementsClosingColgroup.Add("tfoot");
			_htmlElementsClosingColgroup.Add("tbody");
			_htmlElementsClosingDD = new ArrayList();
			_htmlElementsClosingDD.Add("dd");
			_htmlElementsClosingDD.Add("dt");
			_htmlElementsClosingDT = new ArrayList();
			_htmlElementsClosingDD.Add("dd");
			_htmlElementsClosingDD.Add("dt");
			_htmlElementsClosingLI = new ArrayList();
			_htmlElementsClosingLI.Add("li");
			_htmlElementsClosingTbody = new ArrayList();
			_htmlElementsClosingTbody.Add("tbody");
			_htmlElementsClosingTbody.Add("thead");
			_htmlElementsClosingTbody.Add("tfoot");
			_htmlElementsClosingTR = new ArrayList();
			_htmlElementsClosingTR.Add("thead");
			_htmlElementsClosingTR.Add("tfoot");
			_htmlElementsClosingTR.Add("tbody");
			_htmlElementsClosingTR.Add("tr");
			_htmlElementsClosingTD = new ArrayList();
			_htmlElementsClosingTD.Add("td");
			_htmlElementsClosingTD.Add("th");
			_htmlElementsClosingTD.Add("tr");
			_htmlElementsClosingTD.Add("tbody");
			_htmlElementsClosingTD.Add("tfoot");
			_htmlElementsClosingTD.Add("thead");
			_htmlElementsClosingTH = new ArrayList();
			_htmlElementsClosingTH.Add("td");
			_htmlElementsClosingTH.Add("th");
			_htmlElementsClosingTH.Add("tr");
			_htmlElementsClosingTH.Add("tbody");
			_htmlElementsClosingTH.Add("tfoot");
			_htmlElementsClosingTH.Add("thead");
			_htmlElementsClosingThead = new ArrayList();
			_htmlElementsClosingThead.Add("tbody");
			_htmlElementsClosingThead.Add("tfoot");
			_htmlElementsClosingTfoot = new ArrayList();
			_htmlElementsClosingTfoot.Add("tbody");
			_htmlElementsClosingTfoot.Add("thead");
		}

		private static void InitializeHtmlCharacterEntities()
		{
			_htmlCharacterEntities = new Hashtable();
			_htmlCharacterEntities["Aacute"] = 'Á';
			_htmlCharacterEntities["aacute"] = 'á';
			_htmlCharacterEntities["Acirc"] = 'Â';
			_htmlCharacterEntities["acirc"] = 'â';
			_htmlCharacterEntities["acute"] = '\u00b4';
			_htmlCharacterEntities["AElig"] = 'Æ';
			_htmlCharacterEntities["aelig"] = 'æ';
			_htmlCharacterEntities["Agrave"] = 'À';
			_htmlCharacterEntities["agrave"] = 'à';
			_htmlCharacterEntities["alefsym"] = 'ℵ';
			_htmlCharacterEntities["Alpha"] = 'Α';
			_htmlCharacterEntities["alpha"] = 'α';
			_htmlCharacterEntities["amp"] = '&';
			_htmlCharacterEntities["and"] = '∧';
			_htmlCharacterEntities["ang"] = '∠';
			_htmlCharacterEntities["Aring"] = 'Å';
			_htmlCharacterEntities["aring"] = 'å';
			_htmlCharacterEntities["asymp"] = '≈';
			_htmlCharacterEntities["Atilde"] = 'Ã';
			_htmlCharacterEntities["atilde"] = 'ã';
			_htmlCharacterEntities["Auml"] = 'Ä';
			_htmlCharacterEntities["auml"] = 'ä';
			_htmlCharacterEntities["bdquo"] = '„';
			_htmlCharacterEntities["Beta"] = 'Β';
			_htmlCharacterEntities["beta"] = 'β';
			_htmlCharacterEntities["brvbar"] = '¦';
			_htmlCharacterEntities["bull"] = '•';
			_htmlCharacterEntities["cap"] = '∩';
			_htmlCharacterEntities["Ccedil"] = 'Ç';
			_htmlCharacterEntities["ccedil"] = 'ç';
			_htmlCharacterEntities["cent"] = '¢';
			_htmlCharacterEntities["Chi"] = 'Χ';
			_htmlCharacterEntities["chi"] = 'χ';
			_htmlCharacterEntities["circ"] = 'ˆ';
			_htmlCharacterEntities["clubs"] = '♣';
			_htmlCharacterEntities["cong"] = '≅';
			_htmlCharacterEntities["copy"] = '©';
			_htmlCharacterEntities["crarr"] = '↵';
			_htmlCharacterEntities["cup"] = '∪';
			_htmlCharacterEntities["curren"] = '¤';
			_htmlCharacterEntities["dagger"] = '†';
			_htmlCharacterEntities["Dagger"] = '‡';
			_htmlCharacterEntities["darr"] = '↓';
			_htmlCharacterEntities["dArr"] = '⇓';
			_htmlCharacterEntities["deg"] = '°';
			_htmlCharacterEntities["Delta"] = 'Δ';
			_htmlCharacterEntities["delta"] = 'δ';
			_htmlCharacterEntities["diams"] = '♦';
			_htmlCharacterEntities["divide"] = '÷';
			_htmlCharacterEntities["Eacute"] = 'É';
			_htmlCharacterEntities["eacute"] = 'é';
			_htmlCharacterEntities["Ecirc"] = 'Ê';
			_htmlCharacterEntities["ecirc"] = 'ê';
			_htmlCharacterEntities["Egrave"] = 'È';
			_htmlCharacterEntities["egrave"] = 'è';
			_htmlCharacterEntities["empty"] = '∅';
			_htmlCharacterEntities["emsp"] = '\u2003';
			_htmlCharacterEntities["ensp"] = '\u2002';
			_htmlCharacterEntities["Epsilon"] = 'Ε';
			_htmlCharacterEntities["epsilon"] = 'ε';
			_htmlCharacterEntities["equiv"] = '≡';
			_htmlCharacterEntities["Eta"] = 'Η';
			_htmlCharacterEntities["eta"] = 'η';
			_htmlCharacterEntities["ETH"] = 'Ð';
			_htmlCharacterEntities["eth"] = 'ð';
			_htmlCharacterEntities["Euml"] = 'Ë';
			_htmlCharacterEntities["euml"] = 'ë';
			_htmlCharacterEntities["euro"] = '€';
			_htmlCharacterEntities["exist"] = '∃';
			_htmlCharacterEntities["fnof"] = 'ƒ';
			_htmlCharacterEntities["forall"] = '∀';
			_htmlCharacterEntities["frac12"] = '½';
			_htmlCharacterEntities["frac14"] = '¼';
			_htmlCharacterEntities["frac34"] = '¾';
			_htmlCharacterEntities["frasl"] = '⁄';
			_htmlCharacterEntities["Gamma"] = 'Γ';
			_htmlCharacterEntities["gamma"] = 'γ';
			_htmlCharacterEntities["ge"] = '≥';
			_htmlCharacterEntities["gt"] = '>';
			_htmlCharacterEntities["harr"] = '↔';
			_htmlCharacterEntities["hArr"] = '⇔';
			_htmlCharacterEntities["hearts"] = '♥';
			_htmlCharacterEntities["hellip"] = '…';
			_htmlCharacterEntities["Iacute"] = 'Í';
			_htmlCharacterEntities["iacute"] = 'í';
			_htmlCharacterEntities["Icirc"] = 'Î';
			_htmlCharacterEntities["icirc"] = 'î';
			_htmlCharacterEntities["iexcl"] = '¡';
			_htmlCharacterEntities["Igrave"] = 'Ì';
			_htmlCharacterEntities["igrave"] = 'ì';
			_htmlCharacterEntities["image"] = 'ℑ';
			_htmlCharacterEntities["infin"] = '∞';
			_htmlCharacterEntities["int"] = '∫';
			_htmlCharacterEntities["Iota"] = 'Ι';
			_htmlCharacterEntities["iota"] = 'ι';
			_htmlCharacterEntities["iquest"] = '¿';
			_htmlCharacterEntities["isin"] = '∈';
			_htmlCharacterEntities["Iuml"] = 'Ï';
			_htmlCharacterEntities["iuml"] = 'ï';
			_htmlCharacterEntities["Kappa"] = 'Κ';
			_htmlCharacterEntities["kappa"] = 'κ';
			_htmlCharacterEntities["Lambda"] = 'Λ';
			_htmlCharacterEntities["lambda"] = 'λ';
			_htmlCharacterEntities["lang"] = '〈';
			_htmlCharacterEntities["laquo"] = '«';
			_htmlCharacterEntities["larr"] = '←';
			_htmlCharacterEntities["lArr"] = '⇐';
			_htmlCharacterEntities["lceil"] = '⌈';
			_htmlCharacterEntities["ldquo"] = '“';
			_htmlCharacterEntities["le"] = '≤';
			_htmlCharacterEntities["lfloor"] = '⌊';
			_htmlCharacterEntities["lowast"] = '∗';
			_htmlCharacterEntities["loz"] = '◊';
			_htmlCharacterEntities["lrm"] = '\u200e';
			_htmlCharacterEntities["lsaquo"] = '‹';
			_htmlCharacterEntities["lsquo"] = '‘';
			_htmlCharacterEntities["lt"] = '<';
			_htmlCharacterEntities["macr"] = '\u00af';
			_htmlCharacterEntities["mdash"] = '—';
			_htmlCharacterEntities["micro"] = 'µ';
			_htmlCharacterEntities["middot"] = '·';
			_htmlCharacterEntities["minus"] = '−';
			_htmlCharacterEntities["Mu"] = 'Μ';
			_htmlCharacterEntities["mu"] = 'μ';
			_htmlCharacterEntities["nabla"] = '∇';
			_htmlCharacterEntities["nbsp"] = '\u00a0';
			_htmlCharacterEntities["ndash"] = '–';
			_htmlCharacterEntities["ne"] = '≠';
			_htmlCharacterEntities["ni"] = '∋';
			_htmlCharacterEntities["not"] = '¬';
			_htmlCharacterEntities["notin"] = '∉';
			_htmlCharacterEntities["nsub"] = '⊄';
			_htmlCharacterEntities["Ntilde"] = 'Ñ';
			_htmlCharacterEntities["ntilde"] = 'ñ';
			_htmlCharacterEntities["Nu"] = 'Ν';
			_htmlCharacterEntities["nu"] = 'ν';
			_htmlCharacterEntities["Oacute"] = 'Ó';
			_htmlCharacterEntities["ocirc"] = 'ô';
			_htmlCharacterEntities["OElig"] = 'Œ';
			_htmlCharacterEntities["oelig"] = 'œ';
			_htmlCharacterEntities["Ograve"] = 'Ò';
			_htmlCharacterEntities["ograve"] = 'ò';
			_htmlCharacterEntities["oline"] = '‾';
			_htmlCharacterEntities["Omega"] = 'Ω';
			_htmlCharacterEntities["omega"] = 'ω';
			_htmlCharacterEntities["Omicron"] = 'Ο';
			_htmlCharacterEntities["omicron"] = 'ο';
			_htmlCharacterEntities["oplus"] = '⊕';
			_htmlCharacterEntities["or"] = '∨';
			_htmlCharacterEntities["ordf"] = 'ª';
			_htmlCharacterEntities["ordm"] = 'º';
			_htmlCharacterEntities["Oslash"] = 'Ø';
			_htmlCharacterEntities["oslash"] = 'ø';
			_htmlCharacterEntities["Otilde"] = 'Õ';
			_htmlCharacterEntities["otilde"] = 'õ';
			_htmlCharacterEntities["otimes"] = '⊗';
			_htmlCharacterEntities["Ouml"] = 'Ö';
			_htmlCharacterEntities["ouml"] = 'ö';
			_htmlCharacterEntities["para"] = '¶';
			_htmlCharacterEntities["part"] = '∂';
			_htmlCharacterEntities["permil"] = '‰';
			_htmlCharacterEntities["perp"] = '⊥';
			_htmlCharacterEntities["Phi"] = 'Φ';
			_htmlCharacterEntities["phi"] = 'φ';
			_htmlCharacterEntities["pi"] = 'π';
			_htmlCharacterEntities["piv"] = 'ϖ';
			_htmlCharacterEntities["plusmn"] = '±';
			_htmlCharacterEntities["pound"] = '£';
			_htmlCharacterEntities["prime"] = '′';
			_htmlCharacterEntities["Prime"] = '″';
			_htmlCharacterEntities["prod"] = '∏';
			_htmlCharacterEntities["prop"] = '∝';
			_htmlCharacterEntities["Psi"] = 'Ψ';
			_htmlCharacterEntities["psi"] = 'ψ';
			_htmlCharacterEntities["quot"] = '"';
			_htmlCharacterEntities["radic"] = '√';
			_htmlCharacterEntities["rang"] = '〉';
			_htmlCharacterEntities["raquo"] = '»';
			_htmlCharacterEntities["rarr"] = '→';
			_htmlCharacterEntities["rArr"] = '⇒';
			_htmlCharacterEntities["rceil"] = '⌉';
			_htmlCharacterEntities["rdquo"] = '”';
			_htmlCharacterEntities["real"] = 'ℜ';
			_htmlCharacterEntities["reg"] = '®';
			_htmlCharacterEntities["rfloor"] = '⌋';
			_htmlCharacterEntities["Rho"] = 'Ρ';
			_htmlCharacterEntities["rho"] = 'ρ';
			_htmlCharacterEntities["rlm"] = '\u200f';
			_htmlCharacterEntities["rsaquo"] = '›';
			_htmlCharacterEntities["rsquo"] = '’';
			_htmlCharacterEntities["sbquo"] = '‚';
			_htmlCharacterEntities["Scaron"] = 'Š';
			_htmlCharacterEntities["scaron"] = 'š';
			_htmlCharacterEntities["sdot"] = '⋅';
			_htmlCharacterEntities["sect"] = '§';
			_htmlCharacterEntities["shy"] = '­';
			_htmlCharacterEntities["Sigma"] = 'Σ';
			_htmlCharacterEntities["sigma"] = 'σ';
			_htmlCharacterEntities["sigmaf"] = 'ς';
			_htmlCharacterEntities["sim"] = '∼';
			_htmlCharacterEntities["spades"] = '♠';
			_htmlCharacterEntities["sub"] = '⊂';
			_htmlCharacterEntities["sube"] = '⊆';
			_htmlCharacterEntities["sum"] = '∑';
			_htmlCharacterEntities["sup"] = '⊃';
			_htmlCharacterEntities["sup1"] = '¹';
			_htmlCharacterEntities["sup2"] = '²';
			_htmlCharacterEntities["sup3"] = '³';
			_htmlCharacterEntities["supe"] = '⊇';
			_htmlCharacterEntities["szlig"] = 'ß';
			_htmlCharacterEntities["Tau"] = 'Τ';
			_htmlCharacterEntities["tau"] = 'τ';
			_htmlCharacterEntities["there4"] = '∴';
			_htmlCharacterEntities["Theta"] = 'Θ';
			_htmlCharacterEntities["theta"] = 'θ';
			_htmlCharacterEntities["thetasym"] = 'ϑ';
			_htmlCharacterEntities["thinsp"] = '\u2009';
			_htmlCharacterEntities["THORN"] = 'Þ';
			_htmlCharacterEntities["thorn"] = 'þ';
			_htmlCharacterEntities["tilde"] = '\u02dc';
			_htmlCharacterEntities["times"] = '×';
			_htmlCharacterEntities["trade"] = '™';
			_htmlCharacterEntities["Uacute"] = 'Ú';
			_htmlCharacterEntities["uacute"] = 'ú';
			_htmlCharacterEntities["uarr"] = '↑';
			_htmlCharacterEntities["uArr"] = '⇑';
			_htmlCharacterEntities["Ucirc"] = 'Û';
			_htmlCharacterEntities["ucirc"] = 'û';
			_htmlCharacterEntities["Ugrave"] = 'Ù';
			_htmlCharacterEntities["ugrave"] = 'ù';
			_htmlCharacterEntities["uml"] = '\u00a8';
			_htmlCharacterEntities["upsih"] = 'ϒ';
			_htmlCharacterEntities["Upsilon"] = 'Υ';
			_htmlCharacterEntities["upsilon"] = 'υ';
			_htmlCharacterEntities["Uuml"] = 'Ü';
			_htmlCharacterEntities["uuml"] = 'ü';
			_htmlCharacterEntities["weierp"] = '℘';
			_htmlCharacterEntities["Xi"] = 'Ξ';
			_htmlCharacterEntities["xi"] = 'ξ';
			_htmlCharacterEntities["Yacute"] = 'Ý';
			_htmlCharacterEntities["yacute"] = 'ý';
			_htmlCharacterEntities["yen"] = '¥';
			_htmlCharacterEntities["Yuml"] = 'Ÿ';
			_htmlCharacterEntities["yuml"] = 'ÿ';
			_htmlCharacterEntities["Zeta"] = 'Ζ';
			_htmlCharacterEntities["zeta"] = 'ζ';
			_htmlCharacterEntities["zwj"] = '\u200d';
			_htmlCharacterEntities["zwnj"] = '\u200c';
		}
	}
}
