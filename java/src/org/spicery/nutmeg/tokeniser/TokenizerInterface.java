package org.spicery.nutmeg.tokeniser;

import java.util.List;

public interface TokenizerInterface< T > {
	T readToken();
	List<T> toList();
}