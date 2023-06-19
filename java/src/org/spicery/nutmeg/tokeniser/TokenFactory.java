package org.spicery.nutmeg.tokeniser;

public interface TokenFactory<T> {

	T nameToken(String value);
	T stringToken(String value);
	T intToken(int value);
	T endOfFile();
	
}
