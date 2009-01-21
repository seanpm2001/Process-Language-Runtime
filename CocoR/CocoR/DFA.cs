/*-------------------------------------------------------------------------
DFA.cs -- Generation of the Scanner Automaton
Compiler Generator Coco/R,
Copyright (c) 1990, 2004 Hanspeter Moessenboeck, University of Linz
extended by M. Loeberbauer & A. Woess, Univ. of Linz
with improvements by Pat Terry, Rhodes University

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 2, or (at your option) any 
later version.

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License 
for more details.

You should have received a copy of the GNU General Public License along 
with this program; if not, write to the Free Software Foundation, Inc., 
59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

As an exception, it is allowed to write an extension of Coco/R that is
used as a plugin in non-free software.

If not otherwise stated, any source code generated by Coco/R (other than 
Coco/R itself) does not fall under the GNU General Public License.
-------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Text;
using System.Collections;

namespace at.jku.ssw.Coco {

//-----------------------------------------------------------------------------
//  State
//-----------------------------------------------------------------------------

public class State {				// state of finite automaton
	public int nr;						// state number
	public Action firstAction;// to first action of this state
	public Symbol endOf;			// recognized token if state is final
	public bool ctx;					// true if state is reached via contextTrans
	public State next;
	
	public void AddAction(Action act) {
		Action lasta = null, a = firstAction;
		while (a != null && act.typ >= a.typ) {lasta = a; a = a.next;}
		// collecting classes at the beginning gives better performance
		act.next = a;
		if (a==firstAction) firstAction = act; else lasta.next = act;
	}
	
	public void DetachAction(Action act) {
		Action lasta = null, a = firstAction;
		while (a != null && a != act) {lasta = a; a = a.next;}
		if (a != null)
			if (a == firstAction) firstAction = a.next; else lasta.next = a.next;
	}
	
	public void MeltWith(State s) { // copy actions of s to state
		for (Action action = s.firstAction; action != null; action = action.next) {
			Action a = new Action(action.typ, action.sym, action.tc);
			a.AddTargets(action);
			AddAction(a);
		}
	}
	
}

//-----------------------------------------------------------------------------
//  Action
//-----------------------------------------------------------------------------

public class Action {			// action of finite automaton
	public int typ;					// type of action symbol: clas, chr
	public int sym;					// action symbol
	public int tc;					// transition code: normalTrans, contextTrans
	public Target target;		// states reached from this action
	public Action next;
	
	public Action(int typ, int sym, int tc) {
		this.typ = typ; this.sym = sym; this.tc = tc;
	}
	
	public void AddTarget(Target t) { // add t to the action.targets
		Target last = null;
		Target p = target;
		while (p != null && t.state.nr >= p.state.nr) {
			if (t.state == p.state) return;
			last = p; p = p.next;
		}
		t.next = p;
		if (p == target) target = t; else last.next = t;
	}

	public void AddTargets(Action a) { // add copy of a.targets to action.targets
		for (Target p = a.target; p != null; p = p.next) {
			Target t = new Target(p.state);
			AddTarget(t);
		}
		if (a.tc == Node.contextTrans) tc = Node.contextTrans;
	}
	
	public CharSet Symbols(Tab tab) {
		CharSet s;
		if (typ == Node.clas)
			s = tab.CharClassSet(sym).Clone();
		else {
			s = new CharSet(); s.Set(sym);
		}
		return s;
	}
	
	public void ShiftWith(CharSet s, Tab tab) {
		if (s.Elements() == 1) {
			typ = Node.chr; sym = s.First();
		} else {
			CharClass c = tab.FindCharClass(s);
			if (c == null) c = tab.NewCharClass("#", s); // class with dummy name
			typ = Node.clas; sym = c.n;
		}
	}
	
}

//-----------------------------------------------------------------------------
//  Target
//-----------------------------------------------------------------------------

public class Target {				// set of states that are reached by an action
	public State state;				// target state
	public Target next;
	
	public Target (State s) {
		state = s;
	}
}

//-----------------------------------------------------------------------------
//  Melted
//-----------------------------------------------------------------------------

public class Melted {					// info about melted states
	public BitArray set;				// set of old states
	public State state;					// new state
	public Melted next;
	
	public Melted(BitArray set, State state) {
		this.set = set; this.state = state;
	}	
}

//-----------------------------------------------------------------------------
//  Comment
//-----------------------------------------------------------------------------

public class Comment {					// info about comment syntax
	public string start;
	public string stop;
	public bool nested;
	public Comment next;
	
	public Comment(string start, string stop, bool nested) {
		this.start = start; this.stop = stop; this.nested = nested;
	}
	
}

//-----------------------------------------------------------------------------
//  CharSet
//-----------------------------------------------------------------------------

public class CharSet {

	public class Range {
		public int from, to;
		public Range next;
		public Range(int from, int to) { this.from = from; this.to = to; }
	}

	public Range head;

	public bool this[int i] {
		get {
			for (Range p = head; p != null; p = p.next)
				if (i < p.from) return false;
				else if (i <= p.to) return true; // p.from <= i <= p.to
			return false;
		}
	}

	public void Set(int i) {
		Range cur = head, prev = null;
		while (cur != null && i >= cur.from-1) {
			if (i <= cur.to + 1) { // (cur.from-1) <= i <= (cur.to+1)
				if (i == cur.from - 1) cur.from--;
				else if (i == cur.to + 1) {
					cur.to++;
					Range next = cur.next;
					if (next != null && cur.to == next.from - 1) { cur.to = next.to; cur.next = next.next; };
				}
				return;
			}
			prev = cur; cur = cur.next;
		}
		Range n = new Range(i, i);
		n.next = cur;
		if (prev == null) head = n; else prev.next = n;
	}

	public CharSet Clone() {
		CharSet s = new CharSet();
		Range prev = null;
		for (Range cur = head; cur != null; cur = cur.next) {
			Range r = new Range(cur.from, cur.to);
			if (prev == null) s.head = r; else prev.next = r;
			prev = r;
		}
		return s;
	}

	public bool Equals(CharSet s) {
		Range p = head, q = s.head;
		while (p != null && q != null) {
			if (p.from != q.from || p.to != q.to) return false;
			p = p.next; q = q.next;
		}
		return p == q;
	}

	public int Elements() {
		int n = 0;
		for (Range p = head; p != null; p = p.next) n += p.to - p.from + 1;
		return n;
	}

	public int First() {
		if (head != null) return head.from;
		return -1;
	}

	public void Or(CharSet s) {
		for (Range p = s.head; p != null; p = p.next)
			for (int i = p.from; i <= p.to; i++) Set(i);
	}

	public void And(CharSet s) {
		CharSet x = new CharSet();
		for (Range p = head; p != null; p = p.next)
			for (int i = p.from; i <= p.to; i++)
				if (s[i]) x.Set(i);
		head = x.head;
	}

	public void Subtract(CharSet s) {
		CharSet x = new CharSet();
		for (Range p = head; p != null; p = p.next)
			for (int i = p.from; i <= p.to; i++)
				if (!s[i]) x.Set(i);
		head = x.head;
	}

	public bool Includes(CharSet s) {
		for (Range p = s.head; p != null; p = p.next)
			for (int i = p.from; i <= p.to; i++)
				if (!this[i]) return false;
		return true;
	}

	public bool Intersects(CharSet s) {
		for (Range p = s.head; p != null; p = p.next)
			for (int i = p.from; i <= p.to; i++)
				if (this[i]) return true;
		return false;
	}

	public void Fill() {
		head = new Range(Char.MinValue, Char.MaxValue);
	}
}

//-----------------------------------------------------------------------------
//  DFA
//-----------------------------------------------------------------------------

public class DFA {
	public const int  EOF = -1;
	
	private int maxStates;
	private int lastStateNr;   // highest state number
	private State firstState;
	private State lastState;   // last allocated state
	private int lastSimState;  // last non melted state
	private FileStream fram;   // scanner frame input
	private StreamWriter gen;  // generated scanner file
	private Symbol curSy;      // current token to be recognized (in FindTrans)
	private bool dirtyDFA;     // DFA may become nondeterministic in MatchLiteral

	public bool ignoreCase;   // true if input should be treated case-insensitively
	public bool hasCtxMoves;  // DFA has context transitions
	
	// other Coco objects
	private Parser     parser;
	private Tab        tab;
	private Errors     errors;
	private TextWriter trace;

	//---------- Output primitives
	private string Ch(int ch) {
		if (ch < ' ' || ch >= 127 || ch == '\'' || ch == '\\') return Convert.ToString(ch);
		else return String.Format("char('{0}')", (char)ch);
	}
	
	private string ChCond(char ch) {
		return String.Format("ch == {0}", Ch(ch));
	}
	
	private void PutRange(CharSet s) {
		for (CharSet.Range r = s.head; r != null; r = r.next) {
			if (r.from == r.to) { gen.Write("ch == " + Ch(r.from)); }
			else if (r.from == 0) { gen.Write("ch <= " + Ch(r.to)); }
			else { gen.Write("ch >= " + Ch(r.from) + " and ch <= " + Ch(r.to)); }
			if (r.next != null) gen.Write(" or ");
		}
	}
	
	//---------- State handling
	
	State NewState() {
		State s = new State(); s.nr = ++lastStateNr;
		if (firstState == null) firstState = s; else lastState.next = s;
		lastState = s;
		return s;
	}
	
	void NewTransition(State from, State to, int typ, int sym, int tc) {
		Target t = new Target(to);
		Action a = new Action(typ, sym, tc); a.target = t;
		from.AddAction(a);
		if (typ == Node.clas) curSy.tokenKind = Symbol.classToken;
	}
	
	void CombineShifts() {
		State state;
		Action a, b, c;
		CharSet seta, setb;
		for (state = firstState; state != null; state = state.next) {
			for (a = state.firstAction; a != null; a = a.next) {
				b = a.next;
				while (b != null)
					if (a.target.state == b.target.state && a.tc == b.tc) {
						seta = a.Symbols(tab); setb = b.Symbols(tab);
						seta.Or(setb);
						a.ShiftWith(seta, tab);
						c = b; b = b.next; state.DetachAction(c);
					} else b = b.next;
			}
		}
	}
	
	void FindUsedStates(State state, BitArray used) {
		if (used[state.nr]) return;
		used[state.nr] = true;
		for (Action a = state.firstAction; a != null; a = a.next)
			FindUsedStates(a.target.state, used);
	}
	
	void DeleteRedundantStates() {
		State[] newState = new State[lastStateNr + 1];
		BitArray used = new BitArray(lastStateNr + 1);
		FindUsedStates(firstState, used);
		// combine equal final states
		for (State s1 = firstState.next; s1 != null; s1 = s1.next) // firstState cannot be final
			if (used[s1.nr] && s1.endOf != null && s1.firstAction == null && !s1.ctx)
				for (State s2 = s1.next; s2 != null; s2 = s2.next)
					if (used[s2.nr] && s1.endOf == s2.endOf && s2.firstAction == null & !s2.ctx) {
						used[s2.nr] = false; newState[s2.nr] = s1;
					}
		for (State state = firstState; state != null; state = state.next)
			if (used[state.nr])
				for (Action a = state.firstAction; a != null; a = a.next)
					if (!used[a.target.state.nr])
						a.target.state = newState[a.target.state.nr];
		// delete unused states
		lastState = firstState; lastStateNr = 0; // firstState has number 0
		for (State state = firstState.next; state != null; state = state.next)
			if (used[state.nr]) {state.nr = ++lastStateNr; lastState = state;}
			else lastState.next = state.next;
	}
	
	State TheState(Node p) {
		State state;
		if (p == null) {state = NewState(); state.endOf = curSy; return state;}
		else return p.state;
	}
	
	void Step(State from, Node p, BitArray stepped) {
		if (p == null) return;
		stepped[p.n] = true;
		switch (p.typ) {
			case Node.clas: case Node.chr: {
				NewTransition(from, TheState(p.next), p.typ, p.val, p.code);
				break;
			}
			case Node.alt: {
				Step(from, p.sub, stepped); Step(from, p.down, stepped);
				break;
			}
			case Node.iter: case Node.opt: {
				if (p.next != null && !stepped[p.next.n]) Step(from, p.next, stepped);
				Step(from, p.sub, stepped);
				if (p.typ == Node.iter && p.state != from) {
					Step(p.state, p, new BitArray(tab.nodes.Count));
				}
				break;
			}
		}
	}

	// Assigns a state n.state to every node n. There will be a transition from
	// n.state to n.next.state triggered by n.val. All nodes in an alternative
	// chain are represented by the same state.
	// Numbering scheme:
	//  - any node after a chr, clas, opt, or alt, must get a new number
	//  - if a nested structure starts with an iteration the iter node must get a new number
	//  - if an iteration follows an iteration, it must get a new number
	void NumberNodes(Node p, State state, bool renumIter) {
		if (p == null) return;
		if (p.state != null) return; // already visited;
		if (state == null || (p.typ == Node.iter && renumIter)) state = NewState();
		p.state = state;
		if (Tab.DelGraph(p)) state.endOf = curSy;
		switch (p.typ) {
			case Node.clas: case Node.chr: {
				NumberNodes(p.next, null, false);
				break;
			}
			case Node.opt: {
				NumberNodes(p.next, null, false);
				NumberNodes(p.sub, state, true);
				break;
			}
			case Node.iter: {
				NumberNodes(p.next, state, true);
				NumberNodes(p.sub, state, true);
				break;
			}
			case Node.alt: {
				NumberNodes(p.next, null, false);
				NumberNodes(p.sub, state, true);
				NumberNodes(p.down, state, renumIter);
				break;
			}
		}
	}
	
	void FindTrans (Node p, bool start, BitArray marked) {
		if (p == null || marked[p.n]) return;
		marked[p.n] = true;
		if (start) Step(p.state, p, new BitArray(tab.nodes.Count)); // start of group of equally numbered nodes
		switch (p.typ) {
			case Node.clas: case Node.chr: {
				FindTrans(p.next, true, marked);
				break;
			}
			case Node.opt: {
				FindTrans(p.next, true, marked); FindTrans(p.sub, false, marked);
				break;
			}
			case Node.iter: {
				FindTrans(p.next, false, marked); FindTrans(p.sub, false, marked);
				break;
			}
			case Node.alt: {
				FindTrans(p.sub, false, marked); FindTrans(p.down, false, marked);
				break;
			}
		}
	}
	
	public void ConvertToStates(Node p, Symbol sym) {
		curSy = sym;
		if (Tab.DelGraph(p)) parser.SemErr("token might be empty");
		NumberNodes(p, firstState, true);
		FindTrans(p, true, new BitArray(tab.nodes.Count));
		if (p.typ == Node.iter) {
			Step(firstState, p, new BitArray(tab.nodes.Count));
		}
	}
	
	// match string against current automaton; store it either as a fixedToken or as a litToken
	public void MatchLiteral(string s, Symbol sym) {
		s = tab.Unescape(s.Substring(1, s.Length-2));
		int i, len = s.Length;
		State state = firstState;
		Action a = null;
		for (i = 0; i < len; i++) { // try to match s against existing DFA
			a = FindAction(state, s[i]);
			if (a == null) break;
			state = a.target.state;
		}
		// if s was not totally consumed or leads to a non-final state => make new DFA from it
		if (i != len || state.endOf == null) {
			state = firstState; i = 0; a = null;
			dirtyDFA = true;
		}
		for (; i < len; i++) { // make new DFA for s[i..len-1], ML: i is either 0 or len
			State to = NewState();
			NewTransition(state, to, Node.chr, s[i], Node.normalTrans);
			state = to;
		}
		Symbol matchedSym = state.endOf;
		if (state.endOf == null) {
			state.endOf = sym;
		} else if (matchedSym.tokenKind == Symbol.fixedToken || (a != null && a.tc == Node.contextTrans)) {
			// s matched a token with a fixed definition or a token with an appendix that will be cut off
			parser.SemErr("tokens " + sym.name + " and " + matchedSym.name + " cannot be distinguished");
		} else { // matchedSym == classToken || classLitToken
			matchedSym.tokenKind = Symbol.classLitToken;
			sym.tokenKind = Symbol.litToken;
		}
	}
	
	void SplitActions(State state, Action a, Action b) {
		Action c; CharSet seta, setb, setc;
		seta = a.Symbols(tab); setb = b.Symbols(tab);
		if (seta.Equals(setb)) {
			a.AddTargets(b);
			state.DetachAction(b);
		} else if (seta.Includes(setb)) {
			setc = seta.Clone(); setc.Subtract(setb);
			b.AddTargets(a);
			a.ShiftWith(setc, tab);
		} else if (setb.Includes(seta)) {
			setc = setb.Clone(); setc.Subtract(seta);
			a.AddTargets(b);
			b.ShiftWith(setc, tab);
		} else {
			setc = seta.Clone(); setc.And(setb);
			seta.Subtract(setc);
			setb.Subtract(setc);
			a.ShiftWith(seta, tab);
			b.ShiftWith(setb, tab);
			c = new Action(0, 0, Node.normalTrans);  // typ and sym are set in ShiftWith
			c.AddTargets(a);
			c.AddTargets(b);
			c.ShiftWith(setc, tab);
			state.AddAction(c);
		}
	}
	
	bool Overlap(Action a, Action b) {
		CharSet seta, setb;
		if (a.typ == Node.chr)
			if (b.typ == Node.chr) return a.sym == b.sym;
			else {setb = tab.CharClassSet(b.sym); return setb[a.sym];}
		else {
			seta = tab.CharClassSet(a.sym);
			if (b.typ == Node.chr) return seta[b.sym];
			else {setb = tab.CharClassSet(b.sym); return seta.Intersects(setb);}
		}
	}
	
	void MakeUnique(State state) {
		bool changed;
		do {
			changed = false;
			for (Action a = state.firstAction; a != null; a = a.next)
				for (Action b = a.next; b != null; b = b.next)
					if (Overlap(a, b)) { SplitActions(state, a, b); changed = true; }
		} while (changed);
	}
	
	void MeltStates(State state) {
		bool ctx;
		BitArray targets;
		Symbol endOf;
		for (Action action = state.firstAction; action != null; action = action.next) {
			if (action.target.next != null) {
				GetTargetStates(action, out targets, out endOf, out ctx);
				Melted melt = StateWithSet(targets);
				if (melt == null) {
					State s = NewState(); s.endOf = endOf; s.ctx = ctx;
					for (Target targ = action.target; targ != null; targ = targ.next)
						s.MeltWith(targ.state);
					MakeUnique(s);
					melt = NewMelted(targets, s);
				}
				action.target.next = null;
				action.target.state = melt.state;
			}
		}
	}
	
	void FindCtxStates() {
		for (State state = firstState; state != null; state = state.next)
			for (Action a = state.firstAction; a != null; a = a.next)
				if (a.tc == Node.contextTrans) a.target.state.ctx = true;
	}
	
	public void MakeDeterministic() {
		State state;
		lastSimState = lastState.nr;
		maxStates = 2 * lastSimState; // heuristic for set size in Melted.set
		FindCtxStates();
		for (state = firstState; state != null; state = state.next)
			MakeUnique(state);
		for (state = firstState; state != null; state = state.next)
			MeltStates(state);
		DeleteRedundantStates();
		CombineShifts();
	}
	
	public void PrintStates() {
		trace.WriteLine();
		trace.WriteLine("---------- states ----------");
		for (State state = firstState; state != null; state = state.next) {
			bool first = true;
			if (state.endOf == null) trace.Write("               ");
			else trace.Write("E({0,12})", tab.Name(state.endOf.name));
			trace.Write("{0,3}:", state.nr);
			if (state.firstAction == null) trace.WriteLine();
			for (Action action = state.firstAction; action != null; action = action.next) {
				if (first) {trace.Write(" "); first = false;} else trace.Write("                    ");
				if (action.typ == Node.clas) trace.Write(((CharClass)tab.classes[action.sym]).name);
				else trace.Write("{0, 3}", Ch(action.sym));
				for (Target targ = action.target; targ != null; targ = targ.next)
					trace.Write(" {0, 3}", targ.state.nr);
				if (action.tc == Node.contextTrans) trace.WriteLine(" context"); else trace.WriteLine();
			}
		}
		trace.WriteLine();
		trace.WriteLine("---------- character classes ----------");
		tab.WriteCharClasses();
	}
	
//---------------------------- actions --------------------------------

	public Action FindAction(State state, char ch) {
		for (Action a = state.firstAction; a != null; a = a.next)
			if (a.typ == Node.chr && ch == a.sym) return a;
			else if (a.typ == Node.clas) {
				CharSet s = tab.CharClassSet(a.sym);
				if (s[ch]) return a;
			}
		return null;
	}
	
	public void GetTargetStates(Action a, out BitArray targets, out Symbol endOf, out bool ctx) { 
		// compute the set of target states
		targets = new BitArray(maxStates); endOf = null;
		ctx = false;
		for (Target t = a.target; t != null; t = t.next) {
			int stateNr = t.state.nr;
			if (stateNr <= lastSimState) targets[stateNr] = true;
			else targets.Or(MeltedSet(stateNr));
			if (t.state.endOf != null)
				if (endOf == null || endOf == t.state.endOf)
					endOf = t.state.endOf;
				else
					errors.SemErr("Tokens " + endOf.name + " and " + t.state.endOf.name + " cannot be distinguished");
			if (t.state.ctx) {
				ctx = true;
				// The following check seems to be unnecessary. It reported an error
				// if a symbol + context was the prefix of another symbol, e.g.
				//   s1 = "a" "b" "c".
				//   s2 = "a" CONTEXT("b").
				// But this is ok.
				// if (t.state.endOf != null) {
				//   Console.WriteLine("Ambiguous context clause");
				//	 errors.count++;
				// }
			}
		}
	}
	
//------------------------- melted states ------------------------------

	Melted firstMelted;	// head of melted state list
	
	Melted NewMelted(BitArray set, State state) {
		Melted m = new Melted(set, state);
		m.next = firstMelted; firstMelted = m;
		return m;
	}
	
	BitArray MeltedSet(int nr) {
		Melted m = firstMelted;
		while (m != null) {
			if (m.state.nr == nr) return m.set; else m = m.next;
		}
		throw new FatalError("compiler error in Melted.Set");
	}

	Melted StateWithSet(BitArray s) {
		for (Melted m = firstMelted; m != null; m = m.next)
			if (Sets.Equals(s, m.set)) return m;
		return null;
	}

//------------------------ comments --------------------------------

	public Comment firstComment;	// list of comments

	string CommentStr(Node p) {
		StringBuilder s = new StringBuilder();
		while (p != null) {
			if (p.typ == Node.chr) {
				s.Append((char)p.val);
			} else if (p.typ == Node.clas) {
				CharSet set = tab.CharClassSet(p.val);
				if (set.Elements() != 1) parser.SemErr("character set contains more than 1 character");
				s.Append((char)set.First());
			} else parser.SemErr("comment delimiters may not be structured");
			p = p.next;
		}
		if (s.Length == 0 || s.Length > 2) {
			parser.SemErr("comment delimiters must be 1 or 2 characters long");
			s = new StringBuilder("?");
		}
		return s.ToString();
	}
	
	public void NewComment(Node from, Node to, bool nested) {
		Comment c = new Comment(CommentStr(from), CommentStr(to), nested);
		c.next = firstComment; firstComment = c;
	}


//------------------------ scanner generation ----------------------

	void GenComBody(Comment com, string baseIndent) {
		gen.WriteLine(baseIndent + "while true:");
		gen.WriteLine(baseIndent + "\tif {0}:", ChCond(com.stop[0]));
		if (com.stop.Length == 1) {
			gen.WriteLine(baseIndent + "\t\tlevel--");
			gen.WriteLine(baseIndent + "\t\tif level == 0:");
			gen.WriteLine(baseIndent + "\t\t\toldEols = line - line0");
			gen.WriteLine(baseIndent + "\t\t\tNextCh()");
			gen.WriteLine(baseIndent + "\t\t\treturn true");
			gen.WriteLine(baseIndent + "\t\tNextCh()");
		} else {
			gen.WriteLine(baseIndent + "\t\tNextCh()");
			gen.WriteLine(baseIndent + "\t\tif {0}:", ChCond(com.stop[1]));
			gen.WriteLine(baseIndent + "\t\t\tlevel--");
			gen.WriteLine(baseIndent + "\t\t\tif level == 0:");
			gen.WriteLine(baseIndent + "\t\t\t\toldEols = line - line0");
			gen.WriteLine(baseIndent + "\t\t\t\tNextCh()");
			gen.WriteLine(baseIndent + "\t\t\t\treturn true");
			gen.WriteLine(baseIndent + "\t\t\tNextCh()");
		}
		if (com.nested) {
			gen.WriteLine(baseIndent + "\telif {0}:", ChCond(com.start[0])); 
			if (com.start.Length == 1) {
				gen.WriteLine(baseIndent + "\t\tlevel++");
				gen.WriteLine(baseIndent + "\t\tNextCh()");
			} else {
				gen.WriteLine(baseIndent + "\t\tNextCh()");
				gen.WriteLine(baseIndent + "\t\tif {0}:", ChCond(com.start[1]));
				gen.WriteLine(baseIndent + "\t\t\tlevel++");
				gen.WriteLine(baseIndent + "\t\t\tNextCh()");
			}
		}
		gen.WriteLine(baseIndent + "\telif ch == Buffer.EOF:");
		gen.WriteLine(baseIndent + "\t\treturn false");
		gen.WriteLine(baseIndent + "\telse:");
		gen.WriteLine(baseIndent + "\t\tNextCh()");
	}
	
	void GenComment(Comment com, int i) {
		gen.WriteLine();
		gen.WriteLine("\tdef Comment{0}() as bool:", i);
		gen.WriteLine("\t\tlevel as int = 1");
		gen.WriteLine("\t\tpos0 as int = pos");
		gen.WriteLine("\t\tline0 as int = line");
		gen.WriteLine("\t\tcol0 as int = col");
		if (com.start.Length == 1) {
			gen.WriteLine("\t\tNextCh()");
			GenComBody(com, "\t\t");
		} else {
			gen.WriteLine("\t\tNextCh()");
			gen.WriteLine("\t\tif {0}:", ChCond(com.start[1]));
			gen.WriteLine("\t\t\tNextCh()");
			GenComBody(com, "\t\t\t");
			gen.WriteLine("\t\telse:");
			gen.WriteLine("\t\t\tbuffer.Pos = pos0");
			gen.WriteLine("\t\t\tNextCh()");
			gen.WriteLine("\t\t\tline = line0");
			gen.WriteLine("\t\t\tcol = col0");
			gen.WriteLine("\t\treturn false");
		}
	}
	
	void CopyFramePart(string stop) {
		char startCh = stop[0];
		int endOfStopString = stop.Length-1;
		int ch = fram.ReadByte();
		while (ch != EOF)
			if (ch == startCh) {
				int i = 0;
				do {
					if (i == endOfStopString) return; // stop[0..i] found
					ch = fram.ReadByte(); i++;
				} while (ch == stop[i]);
				// stop[0..i-1] found; continue with last read character
				gen.Write(stop.Substring(0, i));
			} else {
				gen.Write((char)ch); ch = fram.ReadByte();
			}
		throw new FatalError("incomplete or corrupt scanner frame file");
	}
	
	string SymName(Symbol sym) {
		if (Char.IsLetter(sym.name[0])) { // real name value is stored in Tab.literals
			foreach (DictionaryEntry e in tab.literals)
				if ((Symbol)e.Value == sym) return (string)e.Key;
		}
		return sym.name;
	}
	
	void GenLiterals () {
		if (ignoreCase) {
			gen.WriteLine("\t\ttokString as string = t.val.ToLower() {");
		} else {
			gen.WriteLine("\t\ttokString as string =  t.val");
		}
		bool first = true;
		foreach (Symbol sym in tab.terminals) {
			if (sym.tokenKind == Symbol.litToken) {
				string name = SymName(sym);
				if (ignoreCase) name = name.ToLower();
				// sym.name stores literals with quotes, e.g. "\"Literal\""
				if (first) {
					gen.WriteLine("\t\tif tokString == {0}: t.kind = {1}", name, sym.n);
				} else {
					gen.WriteLine("\t\telif tokString == {0}: t.kind = {1}", name, sym.n);
				}
				first = false;
			}
		}
		gen.WriteLine("\t\telse: break");
		gen.Write("\t");
	}
	
	void WriteState(State state) {
		Symbol endOf = state.endOf;
		gen.WriteLine("\t\t\telif state == {0}:", state.nr);
		bool ctxEnd = state.ctx;
		for (Action action = state.firstAction; action != null; action = action.next) {
			if (action == state.firstAction) gen.Write("\t\t\t\tif ");
			else gen.Write("\t\t\t\telif ");
			if (action.typ == Node.chr) gen.Write(ChCond((char)action.sym));
			else PutRange(tab.CharClassSet(action.sym));
			gen.WriteLine(":");
			
			if (action.tc == Node.contextTrans) {
				gen.WriteLine("\t\t\t\t\tapx++; "); ctxEnd = false;
			} else if (state.ctx)
				gen.WriteLine("\t\t\t\t\tapx = 0; ");
			gen.WriteLine("\t\t\t\t\tAddCh()");
			gen.WriteLine("\t\t\t\t\tstate = {0}", action.target.state.nr);
			gen.WriteLine("\t\t\t\t\tcontinue");
		}
		string indent = "";
		if (state.firstAction == null){
			indent = "\t\t\t\t";
		} else {
			gen.WriteLine("\t\t\t\telse:");
			indent = "\t\t\t\t\t";
		}
		if (ctxEnd) { // final context state: cut appendix
			gen.WriteLine();
			gen.WriteLine("\t\t\t\t\ttlen -= apx;");
			gen.WriteLine("\t\t\t\t\tbuffer.Pos = t.pos");
			gen.WriteLine("\t\t\t\t\tNextCh()");
			gen.WriteLine("\t\t\t\t\tline = t.line");
			gen.WriteLine("\t\t\t\t\tcol = t.col;");
			gen.WriteLine("\t\t\t\t\tfor i in range(0,tlen): NextCh()");
		}
		if (endOf == null) {
			gen.WriteLine(indent + "t.kind = noSym");
		} else {
			gen.WriteLine(indent + "t.kind = {0} ", endOf.n);
			if (endOf.tokenKind == Symbol.classLitToken) {
				gen.WriteLine(indent + "t.val = String(tval, 0, tlen)");
				gen.WriteLine(indent + "CheckLiteral()");
				gen.WriteLine(indent + "return t");
			}
		}
	}
	
	void WriteStartTab() {
		for (Action action = firstState.firstAction; action != null; action = action.next) {
			int targetState = action.target.state.nr;
			if (action.typ == Node.chr) {
				gen.WriteLine("\t\tstart[" + action.sym + "] = " + targetState + "; ");
			} else {
				CharSet s = tab.CharClassSet(action.sym);
				for (CharSet.Range r = s.head; r != null; r = r.next) {
					gen.WriteLine("\t\tfor i in range(" + r.from + ", " + r.to+1 + "): start[i] = " + targetState);
				}
      }
		}
		gen.WriteLine("\t\tstart[Buffer.EOF] = -1;");
	}
	
	void OpenGen(bool backUp) { /* pdt */
		try {
			string fn = Path.Combine(tab.outDir, "Scanner.boo"); /* pdt */
			if (File.Exists(fn) && backUp) File.Copy(fn, fn + ".old", true);
			gen = new StreamWriter(new FileStream(fn, FileMode.Create)); /* pdt */
		} catch (IOException) {
			throw new FatalError("Cannot generate scanner file");
		}
	}

	public void WriteScanner() {
		int i;
		string fr = Path.Combine(tab.srcDir, "Scanner.frame");  /* pdt */
		if (!File.Exists(fr)) {
			if (tab.frameDir != null) fr = Path.Combine(tab.frameDir.Trim(), "Scanner.frame");
			if (!File.Exists(fr)) throw new FatalError("Cannot find Scanner.frame");
		}
		try {
			fram = new FileStream(fr, FileMode.Open, FileAccess.Read, FileShare.Read);
		} catch (FileNotFoundException) {
			throw new FatalError("Cannot open Scanner.frame.");
		}
		OpenGen(true); /* pdt */
		if (dirtyDFA) MakeDeterministic();
		CopyFramePart("-->begin");
		if (!tab.srcName.ToLower().EndsWith("coco.atg")) {
			gen.Close(); OpenGen(false); /* pdt */
		}
		CopyFramePart("-->namespace");
		if (tab.nsName != null && tab.nsName.Length > 0) {
			gen.Write("namespace ");
			gen.Write(tab.nsName);
			gen.Write(" {");
		}
		CopyFramePart("-->declarations");
		gen.WriteLine("\tstatic final maxT as int = {0}", tab.terminals.Count - 1);
		gen.WriteLine("\tstatic final noSym as int = {0}", tab.noSym.n);
		if (ignoreCase)
			gen.Write("\tvalCh as char;       // current input character (for token.val)");
		CopyFramePart("-->initialization");
		WriteStartTab();
		CopyFramePart("-->casing1");
		if (ignoreCase) {
			gen.WriteLine("\t\tif ch != Buffer.EOF:");
			gen.WriteLine("\t\t\tvalCh = cast(char, ch)");
			gen.WriteLine("\t\t\tch = char.ToLower(cast(char,ch))");
		}
		CopyFramePart("-->casing2");
		gen.Write("\t\t\ttval[tlen++] = ");
		if (ignoreCase) gen.Write("valCh;"); else gen.Write("cast(char,ch)");
		CopyFramePart("-->comments");
		Comment com = firstComment; i = 0;
		while (com != null) {
			GenComment(com, i);
			com = com.next; i++;
		}
		CopyFramePart("-->literals"); GenLiterals();
		CopyFramePart("-->scan1");
		if (tab.ignored.Elements() > 0) { PutRange(tab.ignored); } else { gen.Write("false"); }
		gen.Write(":");
		
		CopyFramePart("-->scan2");
		if (firstComment != null) {
			gen.Write("\t\tif ");
			com = firstComment; i = 0;
			while (com != null) {
				gen.Write(ChCond(com.start[0]));
				gen.Write(" and Comment{0}()", i);
				if (com.next != null) gen.Write(" or ");
				com = com.next; i++;
			}
			gen.Write(":\n\t\t\treturn NextToken()");
		}
		if (hasCtxMoves) { gen.WriteLine(); gen.Write("\t\tapx as int = 0;"); } /* pdt */
		CopyFramePart("-->scan3");
		for (State state = firstState.next; state != null; state = state.next)
			WriteState(state);
		CopyFramePart("$$$");
		if (tab.nsName != null && tab.nsName.Length > 0) gen.Write("}");
		gen.Close();
	}
	
	public DFA (Parser parser) {
		this.parser = parser;
		tab = parser.tab;
		errors = parser.errors;
		trace = parser.trace;
		firstState = null; lastState = null; lastStateNr = -1;
		firstState = NewState();
		firstMelted = null; firstComment = null;
		ignoreCase = false;
		dirtyDFA = false;
		hasCtxMoves = false;
	}
	
} // end DFA

} // end namespace
