namespace DioRed.CoIn;

/// <summary>
/// Command interface
/// </summary>
public interface ICommand { }

/// <summary>
/// Command interface with result
/// </summary>
/// <typeparam name="TResult">Result type</typeparam>
public interface ICommand<TResult> { }