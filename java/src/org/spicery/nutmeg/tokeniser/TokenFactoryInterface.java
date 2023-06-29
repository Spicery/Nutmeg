package org.spicery.nutmeg.tokeniser;

public interface TokenFactoryInterface<T> {

	T nameToken(String original, String value);
	default T nameToken(String value) { return nameToken(value, value); }
	T stringToken(String original, String value, char quote);
	T charToken(String original, String value);
	T symbolToken(String original, String value);
	T intToken(String original, int value);
	T endOfFile();
	
}
