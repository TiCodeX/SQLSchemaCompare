type PromiseResolve<T> = (value?: T | PromiseLike<T>) => void;
type PromiseReject = (reason?: any) => void;