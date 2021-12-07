compile_mode :pop11 +strict;

section $-nutmeg;
vars nutmeg_tree = _;

defclass Constant {
    valueConstant
};
vars procedure newConstant = consConstant;

defclass Id {
    nameId
};
vars procedure newId = consId;

defclass Seq;
lconstant empty_seq = consSeq(0);
define newSeq( n );
    if n == 0 then
        empty_seq
    elseif n == 1 then
        ;;; Simply return
    else
        consSeq( n )
    endif
enddefine;

defclass Apply {
    fnApply,
    argsApply
};
vars procedure newApply = consApply;

defclass Fn {
    nameFn,
    paramsFn,
    bodyFn
};
vars procedure newFn = consFn;

defclass Bind {
    patternBind,
    valueBind
};
vars procedure newBind = consBind;


defclass FixedArity {
    arityFixedArity,
    valueFixedArity
};

endsection;
