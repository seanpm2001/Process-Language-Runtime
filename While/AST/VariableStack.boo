namespace While.AST

import System.Collections.Generic
import While

static class VariableStack:

	private _stack = List[of Dictionary[of string, int]]()
	private _nr = 0

	def PushScope():
		_stack.Add(Dictionary[of string, int]())
		
	def PopScope():
		_stack.RemoveAt(_stack.Count-1)

	def Clear():
		_stack.Clear()
		_nr = 0
		
	def DefineVariable(name as string):
		if _stack.Count == 0:
			raise WhileException("Stack is empty, cannot define variable ${name}")
		if _stack[_stack.Count-1].ContainsKey(name):
			raise WhileException("Variable ${name} is already defined in this scope!")
		_stack[_stack.Count-1].Add(name, _nr)
		_nr++
			

	def GetValue(name as string) as int:
		scope = FindScopeForVariable(name)
		if not scope:
			raise WhileException("Variable ${name} is not in scope!")
		return scope[name]
	
	def IsInScope(name as string) as bool:
		return FindScopeForVariable(name) != null
		
	def IsDeclaredInCurrentScope(name as string) as bool:
		return _stack.Count > 0 and _stack[_stack.Count-1].ContainsKey(name)

	def FindScopeForVariable(name as string) as Dictionary[of string, int]:
		for i in range(_stack.Count-1, -1, -1):
			if _stack[i].ContainsKey(name):
				return _stack[i]
		return null
	

